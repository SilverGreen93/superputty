/*
 * Copyright (c) 2009 - 2015 Jim Radford http://www.jimradford.com
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
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace SuperPutty
{
    /// <summary>
    /// One GET, reported back through a callback so callers that were written against the
    /// old BeginGetResponse/EndGetResponse pattern keep their shape. As there, the callback
    /// runs on a thread pool thread, so anything touching the UI has to marshal.
    /// </summary>
    public class httpRequest
    {
        /// <summary>Identifies this application to the update servers</summary>
        private static readonly string UserAgent = "SuperPuTTY/" + Assembly.GetExecutingAssembly().GetName().Version;
        public void MakeRequest(string url, Action<bool, string> callback)
        {
            Task.Run(async delegate
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
                        HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);
                        response.EnsureSuccessStatusCode();
                        callback(true, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                    }
                }
                catch (Exception ex)
                {
                // Fire the callback so we can inform the user what went wrong
                    callback(false, ex.Message);
            }
            });
        }
    }

}
