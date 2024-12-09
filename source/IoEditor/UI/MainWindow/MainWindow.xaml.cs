using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;

using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace IoEditor.UI.MainWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FoldingManager _foldingManager;
        private XmlFoldingStrategy _xmlFoldingStrategy;


        public MainWindow()
        {
            InitializeComponent();

            SetupEditor(generatedXmlViewer);
            SetupEditor(referenceXmlViewer);
            SetupEditor(targetXmlViewer);
        }

        private void SetupEditor(TextEditor textEditor)
        {
            textEditor.TextArea.DocumentChanged += (s, e) => this.TextArea_DocumentChanged(textEditor, s, e);

            var colors = textEditor.SyntaxHighlighting;

            colors.GetNamedColor("Comment").Foreground = new SimpleHighlightingBrush(Colors.Fuchsia);
            colors.GetNamedColor("CData").Foreground = new SimpleHighlightingBrush(Colors.White);

            colors.GetNamedColor("DocType").Foreground = new SimpleHighlightingBrush(Colors.White);
            colors.GetNamedColor("XmlDeclaration").Foreground = new SimpleHighlightingBrush(Colors.Silver);
            colors.GetNamedColor("XmlTag").Foreground = new SimpleHighlightingBrush(Colors.Wheat);
            colors.GetNamedColor("AttributeName").Foreground = new SimpleHighlightingBrush(Colors.Silver);
            colors.GetNamedColor("AttributeValue").Foreground = new SimpleHighlightingBrush(Colors.GreenYellow);
            colors.GetNamedColor("Entity").Foreground = new SimpleHighlightingBrush(Colors.Yellow);
            colors.GetNamedColor("BrokenEntity").Foreground = new SimpleHighlightingBrush(Colors.Red);
        }

        private void TextArea_DocumentChanged(TextEditor textEditor, object? sender, EventArgs e)
        {
            if (textEditor.TextArea.Document != null)
            {
                _foldingManager = FoldingManager.Install(textEditor.TextArea);
                _xmlFoldingStrategy = new XmlFoldingStrategy();

                textEditor.TextChanged += (s, e) =>
                {
                    if (textEditor.Document != null)
                    {
                        _xmlFoldingStrategy.UpdateFoldings(_foldingManager, textEditor.Document);
                    }
                };
            }
        }
    }
}