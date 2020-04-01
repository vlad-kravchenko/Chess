using System;
using System.Collections.Generic;
using System.Linq;

namespace Chess
{
    public class Game
    {
        int rPicked, cPicked;
        bool picked = false;
        Team prevMove = Team.Black;

        bool wkMoved = false, lwrMoved = false, rwrMoved = false;
        bool bkMoved = false, lbrMoved = false, rbrMoved = false;

        string fen = string.Empty;
        public Cell[,] Map;
        private List<Cell> MapList
        {
            get
            {
                List<Cell> list = new List<Cell>();
                for (int row = 0; row < 8; row++)
                    for (int col = 0; col < 8; col++)
                        list.Add(Map[row, col]);
                return list;
            }
        }

        //w - первый ход за белыми
        //KQkq - разрешена ли рокировка
        //'-' - взятие на проходе не применяется    
        //0 - счётчик полуходов
        //1 - счётчик ходов (считаем, что чёрные уже походили)
        public Game(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
        {
            this.fen = fen;
            Map = new Cell[8, 8];

            var a = fen.Split();
            prevMove = fen.Split()[2] == "W" ? Team.White : Team.Black;
            SetupMap();
        }

        private void SetupMap()
        {
            List<string> mapString = GetMapString(fen);
            for (int i = 0; i < mapString.Count(); i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (mapString[i][j] != '1')
                    {
                        Map[i, j] = new Cell(i, j, (Figure)mapString[i][j]);
                    }
                    else
                    {
                        Map[i, j] = new Cell(i, j, Figure.None);
                    }
                }
            }
        }

        private List<string> GetMapString(string fen)
        {
            var mapString = fen.Split()[0];
            mapString = mapString.Replace("2", "11");
            mapString = mapString.Replace("3", "111");
            mapString = mapString.Replace("4", "1111");
            mapString = mapString.Replace("5", "11111");
            mapString = mapString.Replace("6", "111111");
            mapString = mapString.Replace("7", "1111111");
            mapString = mapString.Replace("8", "11111111");
            return mapString.Split('/').ToList();
        }

        private void ClearAvailable()
        {
            MapList.ForEach(c => c.Available = false);
        }

        private void ClearUnderHit()
        {
            MapList.ForEach(c => c.UnderHit = false);
        }

        private bool InRange(int row, int col)
        {
            return row > -1 && col > -1 && row < 8 && col < 8;
        }

        public bool Move(int row, int col)
        {
            if (InRange(rPicked, cPicked) && InRange(row, col) && Map[row, col].Available && picked)
            {
                ClearUnderHit();
                Cell prev = Map[rPicked, cPicked];
                Cell curr = Map[row, col];
                Figure fig = prev.Figure;
                if (prev.Team == Team.Black && row == 7) fig = Figure.BlackQueen;
                else if (prev.Team == Team.White && row == 0) fig = Figure.WhiteQueen;

                EnPassant(row, col);

                Map[row, col] = new Cell(row, col, fig);
                Map[rPicked, cPicked] = new Cell(rPicked, cPicked, Figure.None);
                ClearAvailable();

                switch (fig)
                {
                    case Figure.WhiteKing:
                        wkMoved = true;
                        break;
                    case Figure.WhiteRook:
                        if (cPicked == 0 && rPicked == 7) lwrMoved = true;
                        else if (cPicked == 7 && rPicked == 7) rwrMoved = true;
                        break;
                    case Figure.BlackKing:
                        bkMoved = true;
                        break;
                    case Figure.BlackRook:
                        if (cPicked == 0 && rPicked == 0) lbrMoved = true;
                        else if (cPicked == 7 && rPicked == 0) rbrMoved = true;
                        break;
                }

                picked = false;
                prevMove = Map[row, col].Team;
                if ((fig == Figure.BlackPawn || fig == Figure.WhitePawn) && Math.Abs(row - rPicked) == 2)
                    UpdateFen(Team.None, Map[row, col]);
                else UpdateFen(Team.None);

                CheckStep();
                return true;
            }
            return false;
        }

        private void CheckStep()
        {
            foreach (var cell in MapList.Where(c => c.Empty == false))
            {
                MarkAvailable(cell);
            }
            var underHit = MapList.Where(c => c.Available && (c.Figure == Figure.BlackKing || c.Figure == Figure.WhiteKing));
            foreach (var cell in underHit) cell.UnderHit = true;
            ClearAvailable();
        }

        private void EnPassant(int row, int col)
        {
            string pass = fen.Split()[3];
            if (pass != "-")
            {
                int r = Convert.ToInt32(pass[1].ToString());
                int c = Convert.ToInt32(pass[0].ToString());
                if (col == c && row == r && prevMove != Map[rPicked, cPicked].Team)
                {
                    if (prevMove == Team.Black)
                        Map[r + 1, c] = new Cell(r + 1, c, Figure.None);
                    else
                        Map[r - 1, c] = new Cell(r - 1, c, Figure.None);
                }
            }
        }

        public bool Castling(int row, int col)
        {
            if (picked && Map[rPicked, cPicked].Team == Map[row, col].Team)
            {
                if ((Map[row, col].IsKing && Map[rPicked, cPicked].IsRook) || (Map[row, col].IsRook && Map[rPicked, cPicked].IsKing))
                {
                    var king = Map[row, col].IsKing ? Map[row, col] : Map[rPicked, cPicked];
                    var rook = Map[row, col].IsRook ? Map[row, col] : Map[rPicked, cPicked];
                    return TryCastle(row, col, king, rook);
                }
            }
            return false;
        }

        private bool TryCastle(int row, int col, Cell king, Cell rook)
        {
            int level = king.Team == Team.Black ? 0 : 7;
            var castleFen = fen.Split()[2];
            if ((castleFen.Contains("k") && rook.Col == 7 && king.Team == Team.Black) || (castleFen.Contains("K") && rook.Col == 7 && king.Team == Team.White))
            {
                if (Map[level, 5].Empty && Map[level, 6].Empty)
                {
                    if (king.Team == Team.Black && (bkMoved || rbrMoved)) return false;
                    if (king.Team == Team.White && (wkMoved || rwrMoved)) return false;

                    Map[level, 6] = new Cell(level, 6, king.Figure);
                    Map[level, 5] = new Cell(level, 5, rook.Figure);
                    Map[rPicked, cPicked] = new Cell(rPicked, cPicked, Figure.None);
                    Map[row, col] = new Cell(row, col, Figure.None);
                    ClearAvailable();
                    picked = false;
                    prevMove = king.Team;
                    if (king.Team == Team.Black)
                    {
                        bkMoved = true;
                        rbrMoved = true;
                    }
                    else
                    {
                        wkMoved = true;
                        rwrMoved = true;
                    }
                    UpdateFen(king.Team);
                    return true;
                }
            }
            else if ((castleFen.Contains("q") && rook.Col == 0 && king.Team == Team.Black) || (castleFen.Contains("Q") && rook.Col == 0 && king.Team == Team.White))
            {
                if (Map[level, 1].Empty && Map[level, 2].Empty && Map[0, 3].Empty)
                {
                    if (king.Team == Team.Black && (bkMoved || lbrMoved)) return false;
                    if (king.Team == Team.White && (wkMoved || lwrMoved)) return false;

                    Map[level, 2] = new Cell(level, 2, king.Figure);
                    Map[level, 3] = new Cell(level, 3, rook.Figure);
                    Map[rPicked, cPicked] = new Cell(rPicked, cPicked, Figure.None);
                    Map[row, col] = new Cell(row, col, Figure.None);
                    ClearAvailable();
                    picked = false;
                    prevMove = king.Team;
                    if (king.Team == Team.Black)
                    {
                        bkMoved = true;
                        lbrMoved = true;
                    }
                    else
                    {
                        wkMoved = true;
                        lwrMoved = true;
                    }
                    UpdateFen(king.Team);
                    return true;
                }
            }
            return false;
        }

        private void UpdateFen(Team team, Cell cell = null)
        {
            string fenNew = string.Empty;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (Map[x, y].Empty)
                        fenNew += "1";
                    else
                        fenNew += (char)Map[x, y].Figure;
                }
                fenNew += "/";
            }
            fenNew += " ";
            fenNew += prevMove == Team.Black ? "w" : "b";
            fenNew += " ";
            string castle = fen.Split()[2];
            if (team == Team.Black)
            {
                if (lbrMoved)
                    castle = castle.Replace("q", "");
                if (rbrMoved)
                    castle = castle.Replace("k", "");
                if (bkMoved)
                {
                    castle = castle.Replace("q", "");
                    castle = castle.Replace("k", "");
                }
            }
            if (team == Team.White)
            {
                if (lwrMoved)
                    castle = castle.Replace("Q", "");
                if (rwrMoved)
                    castle = castle.Replace("k", "");
                if (wkMoved)
                {
                    castle = castle.Replace("Q", "");
                    castle = castle.Replace("K", "");
                }
            }
            if (castle == "") castle = "-";
            fenNew += castle;
            fenNew += " ";
            string oldPass = fen.Split()[3];
            if (cell != null)
            {
                if (cell.Team == Team.Black)
                    oldPass = cell.Col.ToString() + (cell.Row - 1).ToString();
                else
                    oldPass = cell.Col.ToString() + (cell.Row + 1).ToString();
            }
            else oldPass = "-";
            fenNew += oldPass;
            fenNew += " ";
            int halfMoves = Convert.ToInt32(fen.Split()[4]);
            fenNew += (halfMoves + 1).ToString() + " ";
            int moves = Convert.ToInt32(fen.Split()[5]);
            if (prevMove == Team.Black) moves++;
            fenNew += moves.ToString();
            fen = fenNew;
        }

        public bool Pick(int row, int col, bool checkStep = false)
        {
            if (Castling(row, col)) return true;
            if (!InRange(row, col)) return false;
            Cell cell = Map[row, col];
            if (cell.Empty) return false;
            if (picked && Map[row, col].Team != Map[rPicked, cPicked].Team) return false;
            if (!picked && Map[row, col].Team == prevMove) return false;
            ClearAvailable();
            MarkAvailable(cell);
            if (MapList.Any(c => c.Available))
            {
                picked = true;
                rPicked = row;
                cPicked = col;
                return true;
            }
            return false;
        }

        private void MarkAvailable(Cell cell)
        {
            switch (cell.Figure)
            {
                case Figure.WhiteKing:
                case Figure.BlackKing:
                    MarkAvailableKing(cell);
                    break;
                case Figure.WhiteQueen:
                case Figure.BlackQueen:
                    MarkAvailableQueen(cell);
                    break;
                case Figure.WhiteRook:
                case Figure.BlackRook:
                    MarkAvailableRook(cell);
                    break;
                case Figure.WhiteBishop:
                case Figure.BlackBishop:
                    MarkAvailableBishop(cell);
                    break;
                case Figure.WhiteKnight:
                case Figure.BlackKnight:
                    MarkAvailableKnight(cell);
                    break;
                case Figure.WhitePawn:
                    MarkAvailablePawn(cell);
                    break;
                case Figure.BlackPawn:
                    MarkAvailablePawn(cell);
                    break;
            }
        }

        private void MarkAvailableKing(Cell cell)
        {
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (InRange(cell.Row + i, cell.Col + j) && Map[cell.Row + i, cell.Col + j].Team != cell.Team)
                    {
                        Map[cell.Row + i, cell.Col + j].Available = true;
                    }
                }
            }
        }

        private void MarkAvailableQueen(Cell cell)
        {
            MarkAvailableRook(cell);
            MarkAvailableBishop(cell);
        }

        private void MarkAvailableRook(Cell cell)
        {
            for (int row = cell.Row + 1; row < 8; row++)
            {
                if (!CheckDirection(row, cell.Col, cell)) break;
            }
            for (int row = cell.Row - 1; row > -1; row--)
            {
                if (!CheckDirection(row, cell.Col, cell)) break;
            }
            for (int col = cell.Col + 1; col < 8; col++)
            {
                if (!CheckDirection(cell.Row, col, cell)) break;
            }
            for (int col = cell.Col - 1; col > -1; col--)
            {
                if (!CheckDirection(cell.Row, col, cell)) break;
            }
        }

        private void MarkAvailableBishop(Cell cell)
        {
            for (int row = cell.Row - 1, col = cell.Col - 1; row > -1; row--, col--)
            {
                if (!CheckDirection(row, col, cell)) break;
            }
            for (int row = cell.Row + 1, col = cell.Col + 1; row < 8; row++, col++)
            {
                if (!CheckDirection(row, col, cell)) break;
            }
            for (int row = cell.Row - 1, col = cell.Col + 1; row > -1; row--, col++)
            {
                if (!CheckDirection(row, col, cell)) break;
            }
            for (int row = cell.Row + 1, col = cell.Col - 1; row < 8; row++, col--)
            {
                if (!CheckDirection(row, col, cell)) break;
            }
        }

        private void MarkAvailableKnight(Cell cell)
        {
            List<List<int>> steps = new List<List<int>>()
            {
                new List<int>(){-2,-1},
                new List<int>(){-2,1},
                new List<int>(){-1,2},
                new List<int>(){1,2},
                new List<int>(){2,1},
                new List<int>(){2,-1},
                new List<int>(){1,-2},
                new List<int>(){-1,-2},
            };
            foreach (var pair in steps)
            {
                if (!InRange(cell.Row + pair[0], cell.Col + pair[1])) continue;
                if (Map[cell.Row + pair[0], cell.Col + pair[1]].Team == cell.Team) continue;
                Map[cell.Row + pair[0], cell.Col + pair[1]].Available = true;
            }
        }

        private void MarkAvailablePawn(Cell cell)
        {
            if (cell.Figure == Figure.BlackPawn)
            {
                if (InRange(cell.Row + 1, cell.Col) && Map[cell.Row + 1, cell.Col].Empty)
                    Map[cell.Row + 1, cell.Col].Available = true;
                if (InRange(cell.Row + 2, cell.Col) && cell.Row == 1 && Map[cell.Row + 2, cell.Col].Empty)
                    Map[cell.Row + 2, cell.Col].Available = true;
                if (InRange(cell.Row + 1, cell.Col - 1) && Map[cell.Row + 1, cell.Col - 1].Team != cell.Team && !Map[cell.Row + 1, cell.Col - 1].Empty)
                    Map[cell.Row + 1, cell.Col - 1].Available = true;
                if (InRange(cell.Row + 1, cell.Col + 1) && Map[cell.Row + 1, cell.Col + 1].Team != cell.Team && !Map[cell.Row + 1, cell.Col + 1].Empty)
                    Map[cell.Row + 1, cell.Col + 1].Available = true;

            }
            else
            {
                if (InRange(cell.Row - 1, cell.Col) && Map[cell.Row - 1, cell.Col].Empty)
                    Map[cell.Row - 1, cell.Col].Available = true;
                if (InRange(cell.Row - 2, cell.Col) && cell.Row == 6 && Map[cell.Row - 2, cell.Col].Empty)
                    Map[cell.Row - 2, cell.Col].Available = true;
                if (InRange(cell.Row - 1, cell.Col - 1) && Map[cell.Row - 1, cell.Col - 1].Team != cell.Team && !Map[cell.Row - 1, cell.Col - 1].Empty)
                    Map[cell.Row - 1, cell.Col - 1].Available = true;
                if (InRange(cell.Row - 1, cell.Col + 1) && Map[cell.Row - 1, cell.Col + 1].Team != cell.Team && !Map[cell.Row - 1, cell.Col + 1].Empty)
                    Map[cell.Row - 1, cell.Col + 1].Available = true;

            }
        }

        private bool CheckDirection(int row, int col, Cell cell)
        {
            if (InRange(row, col))
            {
                if (Map[row, col].Team == cell.Team) return false;
                if (Map[row, col].Empty)
                {
                    Map[row, col].Available = true;
                    return true;
                }
                else
                {
                    Map[row, col].Available = true;
                    return false;
                }
            }
            return false;
        }
    }
}