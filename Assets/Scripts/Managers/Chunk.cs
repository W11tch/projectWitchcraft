namespace ProjectWitchcraft.Managers
{
    public class Chunk
    {
        public const int Size = 32;

        public ChunkCoord Coord { get; }
        public bool IsDirty { get; set; }

        private readonly GridCell[,] _cells;

        public Chunk(ChunkCoord coord)
        {
            Coord = coord;
            _cells = new GridCell[Size, Size];
        }

        public GridCell GetCell(int localX, int localY) => _cells[localX, localY];

        public GridCell GetOrCreateCell(int localX, int localY)
        {
            if (_cells[localX, localY] == null)
                _cells[localX, localY] = new GridCell();
            return _cells[localX, localY];
        }
    }
}
