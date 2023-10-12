namespace Fiero.Core
{
    public class TrackerChannel
    {
        protected readonly List<TrackerChannelRow> Rows = new();

        public TrackerChannelRow GetRow(int pos) => Rows[pos];
        public void SetRow(int pos, TrackerChannelRow row) => Rows[pos % Rows.Count] = row;
        public void ResetRows()
        {
            for (int i = 0; i < Rows.Count; i++)
            {
                Rows[i] = TrackerChannelRow.Empty();
            }
        }
        public TrackerChannel(int nRows = 64)
        {
            Resize(nRows);
        }

        public void Resize(int newRows)
        {
            int currentRows = Rows.Count;
            // Expand the list
            if (newRows > currentRows)
            {
                Rows.AddRange(Enumerable.Repeat(TrackerChannelRow.Empty(), newRows - currentRows));
            }
            // Shrink the list
            else if (newRows < currentRows)
            {
                Rows.RemoveRange(newRows, currentRows - newRows);
            }
            // No change needed if newRows == currentRows
        }
    }
}
