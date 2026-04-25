using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace SsmsRegions
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("SQL")]
    [TagType(typeof(IOutliningRegionTag))]
    internal sealed class RegionOutliningTaggerProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new RegionOutliningTagger(buffer) as ITagger<T>;
        }

    }
}