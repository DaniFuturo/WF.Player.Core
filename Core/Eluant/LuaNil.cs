using System;

namespace WF.Player.Core.Lua
{
	#if USE_KOPILUA
		using LuaApi = KopiLua.Lua;
	#else
		using LuaApi = LuaNative;
	#endif

    public sealed class LuaNil : LuaValueType
    {
        private static readonly LuaNil instance = new LuaNil();

        public static LuaNil Instance
        {
            get { return instance; }
        }

        private LuaNil() { }

        internal override void Push(LuaRuntime runtime)
        {
            LuaApi.lua_pushnil(runtime.LuaState);
        }

        public override bool ToBoolean()
        {
            return false;
        }

        public override double? ToNumber()
        {
            return null;
        }

        public override string ToString()
        {
            return "nil";
        }

        public override bool Equals(LuaValue other)
        {
            return other == null || other == this;
        }
    }
}

