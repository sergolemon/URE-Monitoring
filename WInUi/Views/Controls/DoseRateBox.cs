using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URE.Views.Controls
{
    public class DoseRateBox : TextBox
    {
        public DoseRateBox()
        {
            BeforeTextChanging += OnBeforeTextChanging;
        }

        private void OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            string normalizedNewText = args.NewText.Replace('.',',');

            if(
                (normalizedNewText.Any(x => x == ',') && normalizedNewText.Split(',')[1].Length > 3) ||
                normalizedNewText.Split(',')[0].Length > 5 ||
                (!string.IsNullOrEmpty(normalizedNewText) && !double.TryParse(normalizedNewText, out var value))
                ) args.Cancel = true;
        }
    }
}
