#region Copyright & License Information
/*
 * Copyright 2007-2014 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Eluant;
using Eluant.ObjectBinding;
using OpenRA.FileFormats;
using OpenRA.Traits;

namespace OpenRA.Scripting
{
	public abstract class ScriptObjectWrapper : IScriptBindable, ILuaTableBinding
	{
		protected abstract string DuplicateKeyError(string memberName);
		protected abstract string MemberNotFoundError(string memberName);

		protected readonly ScriptContext context;
		Dictionary<string, ScriptMemberWrapper> members;

		public ScriptObjectWrapper(ScriptContext context)
		{
			this.context = context;
		}

		protected void Bind(IEnumerable<object> clrObjects)
		{
			members = new Dictionary<string, ScriptMemberWrapper>();
			foreach (var obj in clrObjects)
			{
				var wrappable = ScriptMemberWrapper.WrappableMembers(obj.GetType());
				foreach (var m in wrappable)
				{
					if (members.ContainsKey(m.Name))
						throw new LuaException(DuplicateKeyError(m.Name));

					members.Add(m.Name, new ScriptMemberWrapper(context, obj, m));
				}
			}
		}

		public bool ContainsKey(string key) { return members.ContainsKey(key); }

		public LuaValue this[LuaRuntime runtime, LuaValue keyValue]
		{
			get
			{
				var name = keyValue.ToString();
				ScriptMemberWrapper wrapper;
				if (!members.TryGetValue(name, out wrapper))
					throw new LuaException(MemberNotFoundError(name));

				return wrapper.Get(runtime);
			}

			set
			{
				var name = keyValue.ToString();
				ScriptMemberWrapper wrapper;
				if (!members.TryGetValue(name, out wrapper))
					throw new LuaException(MemberNotFoundError(name));

				wrapper.Set(runtime, value);
			}
		}
	}
}
