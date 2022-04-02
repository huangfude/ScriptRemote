using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ScriptRemote.Core.Utils
{
    public class ControlUtil
    {
		/// <summary>
		/// 获取父控件
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="child"></param>
		/// <returns></returns>
		public static T FindVisualParent<T>(DependencyObject child)
			where T : DependencyObject
		{
			var parentObject = VisualTreeHelper.GetParent(child);
			if (parentObject == null)
				return null;
			T parent = parentObject as T;
			if (parent != null)
				return parent;
			return FindVisualParent<T>(parentObject);
		}

		/// <summary>
		/// 通过名称查找父控件
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static T FindParentObject<T>(DependencyObject obj, string name) where T : FrameworkElement
		{
			DependencyObject parent = VisualTreeHelper.GetParent(obj);

			while (parent != null)
			{
				if (parent is T && (((T)parent).Name == name | string.IsNullOrEmpty(name)))
				{
					return (T)parent;
				}

				parent = VisualTreeHelper.GetParent(parent);
			}

			return null;
		}

		/// <summary>
		/// 查找某种类型的子控件，并返回一个List集合
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static List<T> GetChildObjects<T>(DependencyObject obj) where T : FrameworkElement
		{
			DependencyObject child = null;
			List<T> childList = new List<T>();

			for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
			{
				child = VisualTreeHelper.GetChild(obj, i);

				if (child is T)
				{
					childList.Add((T)child);
				}
				childList.AddRange(GetChildObjects<T>(child));
			}
			return childList;
		}

		/// <summary>
		/// 通过名称查找子控件，并返回一个List集合
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static List<T> GetChildObjectList<T>(DependencyObject obj, string name) where T : FrameworkElement
		{
			DependencyObject child = null;
			List<T> childList = new List<T>();

			for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
			{
				child = VisualTreeHelper.GetChild(obj, i);

				if (child is T && (((T)child).Name == name | string.IsNullOrEmpty(name)))
				{
					childList.Add((T)child);
				}
				childList.AddRange(GetChildObjectList<T>(child, name));
			}
			return childList;
		}

		/// <summary>
		/// 通过名称查找某子控件
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static T GetChildObject<T>(DependencyObject obj, string name) where T : FrameworkElement
		{
			DependencyObject child = null;
			T grandChild = null;

			for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
			{
				child = VisualTreeHelper.GetChild(obj, i);

				if (child is T && (((T)child).Name == name | string.IsNullOrEmpty(name)))
				{
					return (T)child;
				}
				else
				{
					grandChild = GetChildObject<T>(child, name);
					if (grandChild != null)
						return grandChild;
				}
			}
			return null;
		}

	}
}
