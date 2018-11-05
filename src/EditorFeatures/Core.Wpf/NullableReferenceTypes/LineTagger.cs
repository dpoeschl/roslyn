using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editor;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using Roslyn.Utilities;

[Export(typeof(ITaggerProvider))]
[ContentType(ContentTypeNames.CSharpContentType)]
[TagType(typeof(LineTag))]
internal class MyTaggerProvider : ITaggerProvider
{
    public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
    {
        if (buffer == null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        if (Workspace.TryGetWorkspace(buffer.AsTextContainer(), out var workspace))
        {
            return new LineTagger(buffer, workspace) as ITagger<T>;
        }

        return null;
    }
}

internal class LineTag : IGlyphTag
{
    internal SolidColorBrush _color;

    public LineTag(SolidColorBrush color)
    {
        _color = color;
    }
}

internal class LineTagger : ITagger<LineTag>
{
    private Workspace _workspace;
    private ITextBuffer _buffer;

    public LineTagger(Workspace workspace)
    {
        _workspace = workspace;
    }

    public LineTagger(ITextBuffer buffer, Workspace workspace)
    {
        _buffer = buffer;
        _workspace = workspace;
    }

    public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

    public IEnumerable<ITagSpan<LineTag>> GetTags(NormalizedSnapshotSpanCollection spans)
    {
        var document = _buffer.CurrentSnapshot.GetOpenDocumentInCurrentContextWithChanges();
        if (document == null)
        {
            yield break;
        }

        if (!document.TryGetSemanticModel(out var semanticModel))
        {
            yield break;
        }

        var projectSupportsIt = document.GetLanguageService<IFigureOutNullabilityLanguageService>().ProjectSupportsNullability(document.Project);

        if (!document.TryGetText(out var text))
        {
            yield break;
        }

        var contents = text.ToString();

        var enables = GetAllIndexes(contents, "#nullable enable").Select(e => (e, true));
        var disables = GetAllIndexes(contents, "#nullable disable").Select(e => (e, false));

        var intermixedNullableChanges = enables.Union(disables).OrderBy(n => n.e);

        foreach (var span in spans)
        {
            for (int i = span.Span.Start; i < span.Span.End; i++)
            {
                if (!projectSupportsIt)
                {
                    yield return new TagSpan<LineTag>(span, new LineTag(Brushes.DarkGray));
                }
                else
                {
                    var lastStatus = intermixedNullableChanges.Where(c => c.e < i).LastOrNullable()?.Item2 ?? false;

                    yield return new TagSpan<LineTag>(span, new LineTag(lastStatus ? Brushes.Orange : Brushes.Blue));
                }
            }
        }
    }

    public static IEnumerable<int> GetAllIndexes(string source, string matchString)
    {
        matchString = Regex.Escape(matchString);
        foreach (Match match in Regex.Matches(source, matchString))
        {
            yield return match.Index;
        }
    }
}

[Export(typeof(IGlyphFactoryProvider))]
[Name("MyGlyph")]
[Order(Before = "VsTextMarker")]
[ContentType(ContentTypeNames.CSharpContentType)]
[TagType(typeof(LineTag))]
internal sealed class MyGlyphFactoryProvider : IGlyphFactoryProvider
{
    public IGlyphFactory GetGlyphFactory(IWpfTextView view, IWpfTextViewMargin margin)
    {
        return new MyGlyphFactory();
    }
}

internal class MyGlyphFactory : IGlyphFactory
{
    public UIElement GenerateGlyph(IWpfTextViewLine line, IGlyphTag tag)
    {
        var lineTag = tag as LineTag;

        var lineHeight = line.Height;
        var grid = new Grid()
        {
            Width = lineHeight,
            Height = lineHeight
        };

        grid.Children.Add(new Rectangle()
        {
            Fill = lineTag._color,
            Width = lineHeight / 3,
            Height = lineHeight,
            HorizontalAlignment = HorizontalAlignment.Right
        });

        return grid;
    }
}
