﻿using System.Collections.Generic;
using System.Linq;
using UtinyRipper.Classes;

namespace UtinyRipper.AssetExporters
{
	public sealed class PrefabExportCollection : AssetExportCollection, IComparer<Object>
	{
		public PrefabExportCollection(IAssetExporter assetExporter, Object asset) :
			this(assetExporter, CreatePrefab(asset))
		{
		}

		private PrefabExportCollection(IAssetExporter assetExporter, Prefab prefab) :
			base(assetExporter, prefab)
		{
			foreach (EditorExtension @object in prefab.FetchObjects().OrderBy(t => t, this))
			{
				AddObject(@object);
			}
		}

		private static Prefab CreatePrefab(Object obj)
		{
			GameObject go;
			if(obj.ClassID == ClassIDType.GameObject)
			{
				go = (GameObject)obj;
			}
			else
			{
				Component component = (Component)obj;
				go = component.GameObject.GetObject(component.File);
			}
			GameObject root = go.GetRoot();
			return new Prefab(root);
		}

		public override bool IsContains(Object @object)
		{
			if(base.IsContains(@object))
			{
				return true;
			}
			return m_exportIDs.ContainsKey(@object);
		}
		
		public override string GetExportID(Object @object)
		{
			if(@object == Asset)
			{
				return base.GetExportID(@object);
			}
			return m_exportIDs[@object];
		}

		public int Compare(Object obj1, Object obj2)
		{
			if (obj1.ClassID == obj2.ClassID)
			{
				return 0;
			}

			if (IsCoreComponent(obj1))
			{
				if (IsCoreComponent(obj2))
				{
					return obj1.ClassID < obj2.ClassID ? -1 : 1;
				}
				else
				{
					return -1;
				}
			}
			else
			{
				if (IsCoreComponent(obj2))
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}
		}

		public override IEnumerable<Object> Objects
		{
			get
			{
				foreach (Object @object in base.Objects)
				{
					yield return @object;
				}
				foreach (Object comp in m_exportIDs.Keys)
				{
					yield return comp;
				}
			}
		}

		protected override void ExportInner(ProjectAssetContainer container, string filePath)
		{
			AssetExporter.Export(container, Objects, filePath);
		}

		private void AddObject(EditorExtension @object)
		{
			string exportID = ObjectUtils.GenerateExportID(@object, m_exportIDs.Values);
			m_exportIDs.Add(@object, exportID);
		}

		private static bool IsCoreComponent(Object component)
		{
			switch (component.ClassID)
			{
				case ClassIDType.GameObject:
				case ClassIDType.Transform:
				case ClassIDType.RectTransform:
					return true;

				default:
					return false;
			}
		}

		private readonly Dictionary<Object, string> m_exportIDs = new Dictionary<Object, string>();
	}
}
