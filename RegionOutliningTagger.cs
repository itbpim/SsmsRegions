using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace SsmsRegions
{
    internal sealed class RegionOutliningTagger : ITagger<IOutliningRegionTag>
    {
        private readonly ITextBuffer _buffer;

        public RegionOutliningTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _buffer.Changed += Buffer_Changed;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private void Buffer_Changed(object sender, TextContentChangedEventArgs e)
        {
            var snapshot = e.After;
            var span = new SnapshotSpan(snapshot, 0, snapshot.Length);
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(span));
        }

        public IEnumerable<ITagSpan<IOutliningRegionTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans == null || spans.Count == 0)
                yield break;

            ITextSnapshot snapshot = spans[0].Snapshot;
            var regionStarts = new Stack<(ITextSnapshotLine Line, string Label, int Indentation)>();

            foreach (ITextSnapshotLine line in snapshot.Lines)
            {
                string text = line.GetText();
                string trimmed = text.Trim();

                if (trimmed.StartsWith("--#region", StringComparison.OrdinalIgnoreCase))
                {
                    string label = trimmed.Length > 9 ? trimmed.Substring(9).Trim() : string.Empty;
                    int indentation = text.IndexOf("--#region");
                    regionStarts.Push((line, label, indentation));
                    continue;
                }

                if (trimmed.Equals("--#endregion", StringComparison.OrdinalIgnoreCase) && regionStarts.Count > 0)
                {
                    var regionStart = regionStarts.Pop();
                    string label = regionStart.Label;

                    int regionStartPos = regionStart.Line.Start.Position + regionStart.Indentation;
                    int regionEndPos = line.End.Position;

                    if (regionEndPos > regionStartPos)
                    {
                        var span = new SnapshotSpan(snapshot, Span.FromBounds(regionStartPos, regionEndPos));
                        string hoverText = snapshot.GetText(span);
                        string collapsedForm = string.IsNullOrWhiteSpace(label) ? "Region" : "Region: " + label;

                        yield return new TagSpan<IOutliningRegionTag>(
                            span,
                            new OutliningRegionTag(
                                isDefaultCollapsed: true,
                                isImplementation: false,
                                collapsedForm: collapsedForm,
                                collapsedHintForm: hoverText));
                    }
                }
            }
        }
    }
}