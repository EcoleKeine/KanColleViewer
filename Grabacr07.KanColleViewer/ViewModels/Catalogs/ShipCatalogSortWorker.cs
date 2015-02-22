using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.Desktop.Metro.Controls;
using Grabacr07.KanColleViewer.ViewModels.Contents;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;
using Livet;

namespace Grabacr07.KanColleViewer.ViewModels.Catalogs
{
	public class ShipCatalogSortWorker : ViewModel
	{
		private static readonly SortableColumn noneColumn = new SortableColumn { Name = "なし", KeySelector = null };
		private static readonly SortableColumn levelColumn = new SortableColumn { Name = "レベル", KeySelector = x => x.Level, };
		private static readonly SortableColumn expColumn = new SortableColumn { Name = "次のレベルまでの経験値", KeySelector = x => x.Level, };

		public static SortableColumn[] Columns { get; set; }

		static ShipCatalogSortWorker()
		{
			Columns = new[]
			{
				noneColumn,
				new SortableColumn { Name = "ID", KeySelector = x => x.Id, },
				new SortableColumn { Name = "艦種", KeySelector = x => x.Info.ShipType.SortNumber, },
				new SortableColumn { Name = "艦名", KeySelector = x => x.Info.SortId, },
				levelColumn,
				new SortableColumn { Name = "Condition 値", KeySelector = x => x.Condition, },
				new SortableColumn { Name = "索敵", KeySelector = x => x.ViewRange, },
			};
		}

		public SortableColumnSelector[] Selectors { get; private set; }


		public ShipCatalogSortWorker()
		{
			this.UpdateSelectors();
		}


		public void SetTarget(ShipCatalogSortTarget sortTarget, bool reverse)
		{
		}

		public IEnumerable<Ship> Sort(IEnumerable<Ship> ships)
		{
			var selectors = this.Selectors.Where(x => x.Current.KeySelector != null).ToArray();
			if (selectors.Length == 0) return ships;

			var orderedShips = ships.OrderBy(selectors[0].Current.KeySelector);

			for (var i = 1; i < selectors.Length; i++)
			{
				var selector = selectors[i].Current.KeySelector;
				orderedShips = selectors[i].IsDescending ? orderedShips.ThenByDescending(selector) : orderedShips.ThenBy(selector);
			}

			return orderedShips;
		}

		private void UpdateSelectors()
		{
			if (this.Selectors == null)
			{
				this.Selectors = new SortableColumnSelector[5];
				foreach (var item in this.Selectors)
				{
					item.Updated = this.UpdateSelectors;
					item.Current = noneColumn;
				}
			}

			// nonColumn 以外で選択された列
			var selectedItems = new HashSet<SortableColumn>();
			SortableColumnSelector previous = null;

			foreach (var selector in this.Selectors)
			{
				var sortables = Columns.Where(x => !selectedItems.Contains(x)).ToList();
				var current = selector.Current;

				if (previous != null && previous.Current == levelColumn)
				{
					// 直前のソート列がレベルだったら、この列は次のレベルまでの経験値にしてあげる
					sortables.Insert(0, expColumn);
					current = expColumn;
					selector.IsDescending = !previous.IsDescending;
				}

				selector.SelectableColumns = sortables.ToArray();
				selector.SafeUpdate(sortables.Contains(current) ? current : sortables.FirstOrDefault());

				if (selector.Current != noneColumn)
				{
					selectedItems.Add(selector.Current);
				}

				previous = selector;
			}
		}
	}


	public class SortableColumnSelector : ViewModel
	{
		internal Action Updated { get; set; }

		#region SelectedColumn 変更通知プロパティ

		private SortableColumn _Column;

		public SortableColumn Current
		{
			get { return this._Column; }
			set
			{
				if (this._Column != value)
				{
					this._Column = value;
					this.Updated();
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region SelectableColumns 変更通知プロパティ

		private SortableColumn[] _SelectableColumns;

		public SortableColumn[] SelectableColumns
		{
			get { return this._SelectableColumns; }
			set
			{
				if (this._SelectableColumns != value)
				{
					this._SelectableColumns = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region IsDescending 変更通知プロパティ

		private bool _IsDescending;

		public bool IsDescending
		{
			get { return this._IsDescending; }
			set
			{
				if (this._IsDescending != value)
				{
					this._IsDescending = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		internal void SafeUpdate(SortableColumn column)
		{
			this._Column = column;
			this.RaisePropertyChanged("Current");
		}
	}

	public class SortableColumn
	{
		public string Name { get; set; }
		public Func<Ship, int> KeySelector { get; set; }
	}
}
