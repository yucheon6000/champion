﻿// Copyright (c) Jeroen van Pienbroek. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace AdvancedInputFieldPlugin.Editor
{
	public static class GameObjectExtensions
	{
		public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
		{
			return go.AddComponent<T>().GetCopyOf(toAdd) as T;
		}
	}
}
