using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URE.Views.Controls
{
    public class PositiveNumberBox : TextBox
    {
        public PositiveNumberBox()
        {
            BeforeTextChanging += OnBeforeTextChanging;
        }

        private void OnBeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.NewText) && !int.TryParse(args.NewText, out var value)) args.Cancel = true;
        }
    }
}
