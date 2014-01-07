///
/// MemoryConstrainedLuaRuntime.cs
///
/// Author:
/// Chris Howie <me@chrishowie.com>
///
/// Modified 2013 for use with WF.Player.Core:
/// Dirk Weltz <web@weltz-online.de>
/// Brice Clocher <contact@cybisoft.net>
///
/// Copyright (c) 2013 Chris Howie
///
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
///
/// The above copyright notice and this permission notice shall be included in
/// all copies or substantial portions of the Software.
///
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
/// THE SOFTWARE.
///

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace WF.Player.Core.Lua
{
    public class MemoryConstrainedLuaRuntime : LuaRuntime
    {
        // This gets complicated because of GC rules and how the runtime collects delegates.  To make things a bit
        // easier, we will maintain a strong reference to the object that has the allocator method until the runtime
        // gets disposed or finalized.  This way the delegate in question is not eligible for collection until after
        // the runtime is done with it.
        //
        // This doesn't make the "runtime shutting down" case better since all bets are off at that point, but it does
        // keep the runtime and the allocator delegate from being eligible at the same moment.
        private static HashSet<AllocatorState> allocatorStates = new HashSet<AllocatorState>();

        private AllocatorState allocatorState;

        public long MemoryUse
        {
            get {
                CheckDisposed();
                return allocatorState.MemoryUse;
            }
        }

        public long MaxMemoryUse {
            get {
                CheckDisposed();
                return allocatorState.MaxMemoryUse;
            }
            set {
                CheckDisposed();
                allocatorState.MaxMemoryUse = value;
            }
        }

        public MemoryConstrainedLuaRuntime()
        {
        }

        protected override void Dispose(bool disposing)
        {
            try {
                base.Dispose(disposing);
            } finally {
                lock (allocatorStates) {
                    allocatorStates.Remove(allocatorState);
                    allocatorState = null;
                }
            }
        }

        protected override LuaAllocator CreateAllocatorDelegate()
        {
            if (allocatorState == null) {
                allocatorState = new AllocatorState();

                lock (allocatorStates) {
                    allocatorStates.Add(allocatorState);
                }
            }

            return allocatorState.Allocator;
        }

        protected override void OnEnterClr()
        {
            allocatorState.InLua = false;
        }

        protected override void OnEnterLua()
        {
            allocatorState.InLua = true;
        }

        private class AllocatorState
        {
            public long MemoryUse;
            public long MaxMemoryUse = long.MaxValue;
            public bool InLua = false;

            public LuaRuntime.LuaAllocator Allocator { get; private set; }

            public AllocatorState()
            {
                Allocator = Allocate;
            }

            // We can't ever fail when in the CLR, because that would cause a Lua error (and therefore a longjmp) so we
            // maintain a flag indicating which runtime we are in.  If in the CLR then we never fail, but we still keep
            // track of memory allocation.
            //
            // Note that we can never fail when newSize < oldSize; Lua makes the assumption that failure is not possible in
            // that case.
            private IntPtr Allocate(IntPtr userData, IntPtr ptr, IntPtr oldSize, IntPtr newSize)
            {
                long newUse = MemoryUse;

                try {
                    if (oldSize == newSize) {
                        // Do nothing, will return ptr.
                    } else if (oldSize == IntPtr.Zero) {
                        // New allocation.
                        newUse += newSize.ToInt64();

                        if (InLua && newUse > MaxMemoryUse) {
                            newUse = MemoryUse; // Reset newUse.
                            ptr = IntPtr.Zero;
                        } else {
                            ptr = Marshal.AllocHGlobal(newSize);
                        }
                    } else if (newSize == IntPtr.Zero) {
                        // Free allocation.
                        Marshal.FreeHGlobal(ptr);

                        newUse -= oldSize.ToInt64();

                        ptr = IntPtr.Zero;
                    } else {
                        // Resizing existing allocation.
                        newUse += newSize.ToInt64() - oldSize.ToInt64();

                        // We can't fail when newSize < oldSize, Lua depends on that.
                        if (InLua && newSize.ToInt64() > oldSize.ToInt64() && newUse > MaxMemoryUse) {
                            newUse = MemoryUse; // Reset newUse.
                            ptr = IntPtr.Zero;
                        } else {
                            ptr = Marshal.ReAllocHGlobal(ptr, newSize);
                        }
                    }
                } catch {
                    newUse = MemoryUse;
                    ptr = IntPtr.Zero;
                }

                MemoryUse = newUse;
                return ptr;
            }
        }
    }
}

