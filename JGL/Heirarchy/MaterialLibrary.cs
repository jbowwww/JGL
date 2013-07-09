using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.IO;
using System.Text;
using JGL.Resource;
using JGL.Debugging;

namespace JGL.Heirarchy
{
	public class MaterialLibrary : ICollection<Material>
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = new AutoTraceSource(AsyncFileTraceListener.GetOrCreate("JGL"));

		private readonly ConcurrentDictionary<string, Material> _materials = new ConcurrentDictionary<string, Material>();
		
		public ICollection<Material> Materials {
			get { return _materials.Values; }
		}
		
		public MaterialLibrary(Stream s)
		{
			LoadMtlFile(s);
		}
		
		public MaterialLibrary(string filename)
		{
			using (Stream s = File.Open(filename, FileMode.Open))
				LoadMtlFile(s);
		}
		
		public Material this [int index] {
			get { return _materials.Values.ElementAt(index); }
		}
		
		public Material this [string name] {
			get { return _materials[name]; }
		}
		
		#region Collection implementation - ICollection[Material] members, among other extras
		
		/// <summary>
		/// Add the specified item.
		/// </summary>
		/// <param name='item'>
		/// Item.
		/// </param>
		/// <exception cref='InvalidOperationException'>
		/// Is thrown when an operation cannot be performed.
		/// </exception>
		/// <remarks>ICollection[Material] implementation</remarks>
		public void Add(Material item)
		{
			if (!_materials.TryAdd(item.Name, item))
				throw new InvalidOperationException(string.Format("Could not add material \"{0}\" to library", item.Name));
		}
		
		/// <summary>
		/// Clear this instance.
		/// </summary>
		/// <remarks>ICollection[Material] implementation</remarks>
		public void Clear()
		{
			_materials.Clear();
		}
		
		/// <summary>
		/// Contains the specified item.
		/// </summary>
		/// <param name='item'>
		/// If set to <c>true</c> item.
		/// </param>
		/// <remarks>ICollection[Material] implementation</remarks>
		public bool Contains(Material item)
		{
			return _materials.ContainsKey(item.Name);
		}

		/// <summary>
		/// Contains the specified materialName.
		/// </summary>
		/// <param name='materialName'>
		/// If set to <c>true</c> material name.
		/// </param>
		public bool Contains(string materialName)
		{
			return _materials.ContainsKey(materialName);
		}
		
		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name='array'>
		/// Array.
		/// </param>
		/// <param name='arrayIndex'>
		/// Array index.
		/// </param>
		/// <remarks>ICollection[Material] implementation</remarks>
		public void CopyTo(Material[] array, int arrayIndex)
		{
			_materials.Values.CopyTo(array, arrayIndex);
		}
		
		/// <summary>
		/// Remove the specified item.
		/// </summary>
		/// <param name='item'>
		/// If set to <c>true</c> item.
		/// </param>
		/// <exception cref='InvalidOperationException'>
		/// Is thrown when an operation cannot be performed.
		/// </exception>
		/// <remarks>ICollection[Material] implementation</remarks>
		public bool Remove(Material item)
		{
			if (!Contains(item.Name))
				return false;
			Material material;
			if (!_materials.TryRemove(item.Name, out material))
				throw new InvalidOperationException(string.Format("Could not remove material \"{0}\" from library (Contains material = {1}", item.Name, Contains(item)));
			Debug.Assert(material.Name == item.Name);
			return true;
		}
		
		/// <summary>
		/// Gets the count.
		/// </summary>
		/// <value>
		/// The count.
		/// </value>
		/// <remarks>ICollection[Material] implementation</remarks>
		public int Count {
			get { return _materials.Count; }
		}
		
		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>ICollection[Material] implementation</remarks>
		public bool IsReadOnly {
			get { return false; }
		}
		
		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns>
		/// The enumerator.
		/// </returns>
		/// <remarks>IEnumerable[Material] implementation</remarks>
		public IEnumerator<Material> GetEnumerator()
		{
			return _materials.Values.GetEnumerator();
		}
		
		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		/// <returns>
		/// The enumerator.
		/// </returns>
		/// <remarks>IEnumerable[Material] implementation</remarks>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator() as IEnumerator;
		}
		
		#endregion
		
		/// <summary>
		/// Loads the mtl file.
		/// </summary>
		/// <param name='fs'>
		/// Fs.
		/// </param>
		public void LoadMtlFile(Stream fs)
		{
			using (StreamReader sr = new StreamReader(fs))
			{
				string line;
				string[] tokens;
				Material m = null;
				while (!sr.EndOfStream)
				{
					line = sr.ReadLine();
					tokens = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					if (tokens.Length > 0)
					{
						switch (tokens[0].ToLower())
						{
							case "newmtl":
								Debug.Assert(tokens.Length == 2);
								m = new Material(tokens[1]);
								Add(m);
								break;
							case "ka":
								Debug.Assert(tokens.Length == 4 && m != null);
								m.Ambient = new OpenTK.Graphics.Color4(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3]), 1);
								break;
							case "kd":
								Debug.Assert(tokens.Length == 4 && m != null);
								m.Diffuse = new OpenTK.Graphics.Color4(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3]), 1);
								break;
							case "ks":
								Debug.Assert(tokens.Length == 4 && m != null);
								m.Specular = new OpenTK.Graphics.Color4(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3]), 1);
								break;
							case "tf":
								Debug.Assert(tokens.Length == 4 && m != null);
								m.TransparencyFilter = new OpenTK.Graphics.Color4(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3]), 1);
								break;
							case "d":
								Debug.Assert(tokens.Length == 2 || tokens.Length == 3 && m != null);
								if (tokens.Length == 3)
								{
									m.Transparency = float.Parse(tokens[2]);
									if (tokens[2].ToLower() == "-halo")
										m.TransparencyHalo = true;
									else
										throw new InvalidDataException(string.Concat("Invalid .MTL transparency specification: ", line));
								}
								else
									m.Transparency = float.Parse(tokens[1]);
								break;
							case "ns":
								Debug.Assert(tokens.Length == 2 && m != null);
								m.SpecularExponent = int.Parse(tokens[1]);
								break;
							case "sharpness":
								Debug.Assert(tokens.Length == 2 && m != null);
								m.ReflectionSharpness = int.Parse(tokens[1]);
								break;
							case "ni":
								Debug.Assert(tokens.Length == 2 && m != null);
								m.OpticalDensity = double.Parse(tokens[1]);
								break;
							case "map_ka":
								Debug.Assert(tokens.Length == 2 && m != null);
								m.TextureAmbient = new Texture(tokens[1]);
								break;
							case "map_kd":
								Debug.Assert(tokens.Length == 2 && m != null);
								m.TextureDiffuse = new Texture(tokens[1]);
								break;
							case "map_ks":
								Debug.Assert(tokens.Length == 2 && m != null);
								m.TextureSpecular = new Texture(tokens[1]);
								break;
							
						}
					}
				}
			}
		}		
	}
}

