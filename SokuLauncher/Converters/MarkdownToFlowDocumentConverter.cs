using Markdig;
using Neo.Markdig.Xaml;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SokuLauncher.Converters
{
    public class MarkdownToFlowDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var content = value as string ?? "";
            var doc = MarkdownXaml.ToFlowDocument(content,
                new MarkdownPipelineBuilder()
                .UseXamlSupportedExtensions()
                .UseAutoLinks(new Markdig.Extensions.AutoLinks.AutoLinkOptions { OpenInNewWindow=true })
                .Build()
            );
            doc.FontSize = 12;
            doc.PagePadding = new Thickness(0);
            return doc;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
