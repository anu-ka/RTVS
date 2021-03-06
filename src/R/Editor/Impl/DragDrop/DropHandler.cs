﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Formatting;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.DragDrop;

namespace Microsoft.R.Editor.DragDrop {
    /// <summary>
    /// Handles file drop on the editor. Primarily from Solution Explorer
    /// </summary>
    internal sealed class DropHandler : IDropHandler {
        private readonly IWpfTextView _wpfTextView;
        private readonly ICoreShell _shell;
        private readonly IRInteractiveWorkflowProvider _workflowProvider;

        public DropHandler(IWpfTextView wpfTextView, ICoreShell shell, IRInteractiveWorkflowProvider workflowProvider) {
            _wpfTextView = wpfTextView;
            _shell = shell;
            _workflowProvider = workflowProvider;
        }

        #region IDropHandler
        public DragDropPointerEffects HandleDataDropped(DragDropInfo dragDropInfo) {
            Task.Run(async () => {
                var folder = await GetRUserFolder();
                _shell.MainThread().Post(() => HandleDrop(dragDropInfo, folder));
            }).DoNotWait();
            return DragDropPointerEffects.None;
        }
        public void HandleDragCanceled() { }
        public DragDropPointerEffects HandleDragStarted(DragDropInfo dragDropInfo) => DragDropPointerEffects.Copy | DragDropPointerEffects.Track;
        public DragDropPointerEffects HandleDraggingOver(DragDropInfo dragDropInfo) => DragDropPointerEffects.Copy | DragDropPointerEffects.Track;
        public bool IsDropEnabled(DragDropInfo dragDropInfo) => true;
        #endregion

        private void HandleDrop(DragDropInfo dragDropInfo, string userFolder) {
            var dataObject = dragDropInfo.Data;

            var document = REditorDocument.FindInProjectedBuffers(_wpfTextView.TextBuffer);
            Debug.Assert(document != null, "Text view does not have associated R document.");

            var text = dataObject.GetPlainText(userFolder, dragDropInfo.KeyStates);
            var line = _wpfTextView.TextViewLines.GetTextViewLineContainingYCoordinate(dragDropInfo.Location.Y);
            line = line ?? _wpfTextView.TextViewLines.LastVisibleLine;
            if (line == null) {
                return;
            }

            var bufferPosition = line.GetBufferPositionFromXCoordinate(dragDropInfo.Location.X);
            bufferPosition = bufferPosition ?? line.End;

            var textBuffer = _wpfTextView.TextBuffer;
            var dropPosition = bufferPosition.Value;

            if (REditorSettings.FormatOnPaste) {
                _wpfTextView.Caret.MoveTo(dropPosition);
            }

            if (text.StartsWithOrdinal(Environment.NewLine) && Whitespace.IsNewLineBeforePosition(new TextProvider(textBuffer.CurrentSnapshot), dropPosition)) {
                text = text.TrimStart();
            }

            var es = _shell.GetService<IApplicationEditorSupport>();
            using (var undoAction = es.CreateCompoundAction(_wpfTextView, textBuffer)) {
                undoAction.Open(Resources.DragDropOperation);
                textBuffer.Replace(new Span(dropPosition, 0), text);

                if (REditorSettings.FormatOnPaste) {
                    RangeFormatter.FormatRange(_wpfTextView, document.TextBuffer, new TextRange(dropPosition, text.Length), REditorSettings.FormatOptions, _shell);
                }

                if (_wpfTextView.Selection != null) {
                    _wpfTextView.Caret.MoveTo(_wpfTextView.Selection.End);
                }
                undoAction.Commit();
            }
        }

        private Task<string> GetRUserFolder() {
            var session = _workflowProvider.GetOrCreate()?.RSession;
            return session?.GetRUserDirectoryAsync();
        }
    }
}
