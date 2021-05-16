using SFML.System;
using System;
using System.Collections.Generic;

namespace Fiero.Core
{
    public class TrackerChannel
    {
        protected readonly List<TrackerChannelRow> Rows = new();

        public TrackerChannelRow GetRow(int pos) => Rows[pos % Rows.Count];
        public void SetRow(int pos, TrackerChannelRow row) => Rows[pos % Rows.Count] = row;
        public void ResetRows()
        {
            for (int i = 0; i < Rows.Count; i++) {
                Rows[i] = TrackerChannelRow.Empty((byte)(i + 1));
            }
        }
        public TrackerChannel(int nRows = 64)
        {
            Resize(nRows);
        }

        public void Resize(int newRows)
        {
            if(newRows > Rows.Count) {
                for (int i = 0; i < newRows - Rows.Count; i++) {
                    Rows.Add(TrackerChannelRow.Empty((byte)Rows.Count));
                }
            }
            else if(newRows < Rows.Count) {
                Rows.RemoveRange(newRows, Rows.Count - newRows);
            }
        }
    }
}
