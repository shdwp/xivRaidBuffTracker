using System.Numerics;

namespace RaidBuffTracker.UI.Layout;

public sealed class HorizontalCategoryTableTrackerWidgetLayout : ITrackerWidgetLayout
	{
		// Token: 0x06000087 RID: 135 RVA: 0x000044B7 File Offset: 0x000026B7
		public void StartOver(Vector2 cellSize, Vector2 cellMargin, Vector2 headerSize, Vector2 viewportSize)
		{
			_cellSize = cellSize;
			_cellMargin = cellMargin;
			_headerSize = headerSize;
			_viewportSize = viewportSize;
			_pos = new Vector2(cellMargin.X, cellMargin.Y + 25f);
		}

		// Token: 0x06000088 RID: 136 RVA: 0x000044F4 File Offset: 0x000026F4
		public Vector2 GetPos(Vector2 offset = default(Vector2))
		{
			return _pos + offset;
		}

		// Token: 0x06000089 RID: 137 RVA: 0x00004514 File Offset: 0x00002714
		public void AdvanceToNextCell()
		{
			bool flag = _pos.X + _cellSize.X * 2f + _cellMargin.X * 2f > _viewportSize.X;
			if (flag)
			{
				_pos.X = _cellMargin.X;
				_pos.Y = _pos.Y + (_cellSize.Y + _cellMargin.Y);
			}
			else
			{
				_pos.X = _pos.X + (_cellSize.X + _cellMargin.X);
			}
		}

		// Token: 0x0600008A RID: 138 RVA: 0x000045CC File Offset: 0x000027CC
		public void AdvancePastHeader()
		{
			_pos.X = _pos.X + (_cellSize.X / 2f + _headerSize.X / 2f + _cellMargin.X);
		}

		// Token: 0x0600008B RID: 139 RVA: 0x00004618 File Offset: 0x00002818
		public void AdvanceToNextSection()
		{
			_pos.X = _cellMargin.X;
			_pos.Y = _pos.Y + (_cellSize.Y + _cellMargin.Y);
		}

		// Token: 0x040000A1 RID: 161
		private Vector2 _pos;

		// Token: 0x040000A2 RID: 162
		private Vector2 _cellSize;

		// Token: 0x040000A3 RID: 163
		private Vector2 _cellMargin;

		// Token: 0x040000A4 RID: 164
		private Vector2 _headerSize;

		// Token: 0x040000A5 RID: 165
		private Vector2 _viewportSize;
	}