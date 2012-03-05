using System;
using ICSharpCode.TextEditor.Document;

namespace ICSharpCode.TextEditor.Util {
    public class IndentOperationInfo {
        public int CurrentIndentWidth;
        public int LineOffset;
        public int StartOffset;
        public int FirstNonSpaceOffset;
        public int CharsRemoved;
        public int CharsInserted;
        public int IndentWidth;
        
        public int StartColumn {
            get {
                return StartOffset - LineOffset;
            }
        }
        
        public IndentOperationInfo(IDocument document, LineSegment line) {
            IndentWidth = document.TextEditorProperties.IndentationSize;
            CurrentIndentWidth = 0;
            
            FirstNonSpaceOffset = LineOffset = line.Offset;
            char c;
            while (IsIndentChar(c = document.GetCharAt(FirstNonSpaceOffset))) {
                FirstNonSpaceOffset++;
                switch (c) {
                    case ' ':
                        CurrentIndentWidth++;
                        break;
                    case '\t':
                        CurrentIndentWidth += (IndentWidth - (CurrentIndentWidth % IndentWidth));
                        break;
                }
            }
        }
        
        public static bool IsIndentChar(char c) {
            return (c == ' ' || c == '\t');
        }
        
        public TextLocation AdjustTextLocation(TextLocation position) {
            // Make the text position stay at the same place in the line after the indentation
            // adjustment, regardless of where it is now.
            if (position.Column >= StartColumn) {
                position.Column -= Math.Min(position.Column - StartColumn, CharsRemoved);
                position.Column += CharsInserted;
            }
            return position;
        }
    }
    
    public class IndentHelper {
        private IDocument document;
        
        private int tabIndent {
            get {
                return document.TextEditorProperties.IndentationSize;
            }
        }
        
        public IndentHelper(IDocument document) {
            this.document = document;
        }
        
        public IndentOperationInfo UnIndentLine(int lineNum) {
            LineSegment line = document.GetLineSegment(lineNum);
            IndentOperationInfo info = new IndentOperationInfo(document, line);
            info.StartOffset = info.FirstNonSpaceOffset;
            
            if (line.Length > 0) {
                if (info.CurrentIndentWidth > 0) {
                    if ((info.CurrentIndentWidth % info.IndentWidth) == 0) {
                        // Remove a full indentation level from the end of the line's leading
                        // whitespace.  This could be any of the following:
                        //  - One tab + (n < tabIndent) spaces
                        //  - One tab
                        //  - tabIndent spaces
                        while (info.CharsRemoved < info.IndentWidth) {
                            info.CharsRemoved++;
                            info.StartOffset--;
                            if (document.GetCharAt(info.StartOffset) == '\t') {
                                break;
                            }
                        }
                    } else {
                        // The current line is indented "in between" indentation levels, which
                        // means we just need to remove (currentIndent % tabIndent) spaces.
                        info.CharsRemoved = (info.CurrentIndentWidth % info.IndentWidth);
                        info.StartOffset -= info.CharsRemoved;
                    }
                    if (info.CharsRemoved > 0) {
                        // Remove the character(s) from the end of the line's leading whitespace.
                        document.Remove(info.StartOffset, info.CharsRemoved);
                    }
                }
            }
            
            return info;
        }
        
        public IndentOperationInfo IndentLine(int lineNum) {
            LineSegment line = document.GetLineSegment(lineNum);
            IndentOperationInfo info = new IndentOperationInfo(document, line);
            info.StartOffset = info.FirstNonSpaceOffset;
            
            if ((info.CurrentIndentWidth % info.IndentWidth) != 0) {
                // Remove partial indentation from end of line
                IndentOperationInfo temp = UnIndentLine(lineNum);
                info.StartOffset = temp.StartOffset;
                info.CharsInserted -= temp.CharsRemoved;
            }
            string indentString = GetIndentationString(document);
            document.Insert(info.StartOffset, indentString);
            info.CharsInserted += indentString.Length;
            
            return info;
        }
        
        public static string GetIndentationString(IDocument document) {
            if (document.TextEditorProperties.ConvertTabsToSpaces) {
                return new string(' ', document.TextEditorProperties.IndentationSize);
            } else {
                return "\t";
            }
        }
    }
}
