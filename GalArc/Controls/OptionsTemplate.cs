using System.Windows.Forms;

namespace GalArc.Controls
{
    /// <summary>
    /// The template for the extra options.
    /// </summary>
    public partial class OptionsTemplate : UserControl
    {
        public string Version { get; set; }

        public string ChooseFile(string pattern = "")
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = string.IsNullOrWhiteSpace(pattern) ? "All files (*.*)|*.*" : pattern;
                if (openFileDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(openFileDialog.FileName))
                {
                    return openFileDialog.FileName;
                }
                return null;
            }
        }

        public string ChooseFolder()
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    return folderBrowserDialog.SelectedPath;
                }
                return null;
            }
        }

        public OptionsTemplate()
        {
            InitializeComponent();
        }
    }
}
