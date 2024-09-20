// File: Log/Controller/Localization.cs
// Date: 2024/08/27
// Description: Log窗体的本地化类，提供设置Culture和更新文本的静态方法
//
// Copyright (C) 2024 detached64
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Log.Controller
{
    public class Localize
    {
        public static void SetLocalCulture(string LocalCulture)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(LocalCulture);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(LocalCulture);
        }

        public static void GetStrings_Log()
        {
            LogWindow.Instance.log_btnHide.Text = Properties.Resources.log_btn_Hide;
            LogWindow.Instance.log_btnClear.Text = Properties.Resources.log_btn_Clear;
            LogWindow.Instance.log_btnResize.Text = Properties.Resources.log_btn_Resize;

            LogWindow.Instance.log_chkbxSave.Text = Properties.Resources.log_chkbx_SaveLog;
            LogWindow.Instance.log_chkbxDebug.Text = Properties.Resources.log_chkbx_Debug;
        }
    }
}
