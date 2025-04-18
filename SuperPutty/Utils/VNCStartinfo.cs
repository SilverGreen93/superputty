/*
 * Copyright (c) 2017 Anish Sane https://stackoverflow.com/users/793796/anishsane
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions: 
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using log4net;
using SuperPutty.Data;
using System.Text.RegularExpressions;
using System.IO;

namespace SuperPutty.Utils
{

    /// <summary>
    /// Helper class for VNC support
    /// </summary>
    public class VNCStartInfo
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(VNCStartInfo));

        private SessionData session;

        public VNCStartInfo(SessionData session)
        {
            this.session = session;

            this.Args = "-host=\"" + session.Host + "\"";

            if (session.Port != 0)
                this.Args += " -port=" + session.Port.ToString();

            if (!String.IsNullOrEmpty(session.Password))
                this.Args += " -password=\"" + session.Password + "\"";

            this.Args += " -scale=auto";

            if (!String.IsNullOrEmpty(session.ExtraArgs))
                this.Args += " " + session.ExtraArgs;

            this.StartingDir = "%userprofile%\\Desktop";
        }

        public string Args { get; set; }
        public string StartingDir { get; set; }

    }
}
