using Xunit;
using Domain.Entities;
using Aplication.Services;

namespace DreameHouse.Tests
{
    public class Match3BoardTests
    {
        [Fact]
        public void Test_SimpleLogicGameMatch3Board()
        {
            // Входные данные:
            // W F 
            // L P 
            // M W 
            // L P 
            // L P 
            // P L 
            // Ожидаемые (выходные) данные: (R - random)
            // R R 
            // R R 
            // R R 
            // W F 
            // L P 
            // M W 

            var board = new Match3BoardService(null, 6, 2);
            board.Board = new Tile[,]
            {
                { T(TileType.WREATH), T(TileType.FLOWER) },
                { T(TileType.LAMP), T(TileType.PILLOW) },
                { T(TileType.MUG), T(TileType.WREATH) },
                { T(TileType.LAMP), T(TileType.PILLOW) },
                { T(TileType.LAMP), T(TileType.PILLOW) },
                { T(TileType.PILLOW), T(TileType.LAMP) }
            };

            board.TrySwapAndProcess(5, 0, 5, 1);

            Assert.Equal(TileType.WREATH, board.Board[3, 0]?.Type);
            Assert.Equal(TileType.LAMP, board.Board[4, 0]?.Type);
            Assert.Equal(TileType.MUG, board.Board[5, 0]?.Type);

            Assert.Equal(TileType.FLOWER, board.Board[3, 1]?.Type);
            Assert.Equal(TileType.PILLOW, board.Board[4, 1]?.Type);
            Assert.Equal(TileType.WREATH, board.Board[5, 1]?.Type);
        }

        [Fact]
        public void Test_CreateBonusAfterSwap()
        {
            // Входные данные:
            // C C F C C
            // F B C B W
            // Ожидаемые: R R H R R (R - random)
            //            F B F B W

            var board = new Match3BoardService(null, 2, 5);
            board.Board = new Tile[,]
            {
                { T(TileType.CAT), T(TileType.CAT), T(TileType.FLOWER), T(TileType.CAT), T(TileType.CAT) },
                { T(TileType.FLOWER), T(TileType.BRICK), T(TileType.CAT), T(TileType.BRICK), T(TileType.WREATH) }
            };

            board.TrySwapAndProcess(0, 2, 1, 2);
            Assert.Equal(TileType.HOME, board.Board[0, 2]?.Type);
        }

        [Fact]
        public void Test_ClearColorAfterBonusActivation()
        {
            // Входные данные:
            // B C H B C
            // F B F B W
            // Ожидаемые: N C N N C (N - None)
            //            F N F N W

            var board = new Match3BoardService(null, 2, 5);
            board.Board = new Tile[,]
            {
                { T(TileType.BRICK), T(TileType.CAT), T(TileType.HOME), T(TileType.BRICK), T(TileType.CAT) },
                { T(TileType.FLOWER), T(TileType.BRICK), T(TileType.FLOWER), T(TileType.BRICK), T(TileType.WREATH) }
            };

            board.ActivateBonusAndClearColor(0, 2, 0, 3);

            for (int r = 0; r < 2; r++)
                for (int c = 0; c < 5; c++)
                    if (board.Board[r, c] != null)
                        Assert.NotEqual(TileType.BRICK, board.Board[r, c]?.Type);
        }

        [Fact]
        public void Test_CreateBonusBomb()
        {
            // Входные данные:
            // B B W B
            // F W B B
            // Ожидаемые: R R B R (R - random)
            //            F W W B

            var board = new Match3BoardService(null, 2, 4);
            board.Board = new Tile[,]
            {
                { T(TileType.BRICK), T(TileType.BRICK), T(TileType.WREATH), T(TileType.BRICK) },
                { T(TileType.FLOWER), T(TileType.WREATH), T(TileType.BRICK), T(TileType.BRICK) }
            };

            board.TrySwapAndProcess(0, 2, 1, 2);
            Assert.Equal(TileType.BOMB, board.Board[0, 2]?.Type);
        }

        [Fact]
        public void Test_ClearAfterBonusCornerBomb()
        {
            // Входные данные:
            // B L
            // F W
            // Ожидаемые: N N
            //            N N

            var board = new Match3BoardService(null, 2, 2);
            board.Board = new Tile[,]
            {
                { T(TileType.BOMB), T(TileType.LAMP) },
                { T(TileType.FLOWER), T(TileType.WREATH) }
            };

            board.ActivateBombAndClear(0, 0);

            for (int r = 0; r < 2; r++)
                for (int c = 0; c < 2; c++)
                    Assert.Null(board.Board[r, c]);
        }

        [Fact]
        public void Test_ClearAfterBonusCenterBomb()
        {
            // Входные данные:
            // F W M
            // P B L
            // M F W
            // Ожидаемые: всё N

            var board = new Match3BoardService(null, 3, 3);
            board.Board = new Tile[,]
            {
                { T(TileType.FLOWER), T(TileType.WREATH), T(TileType.MUG) },
                { T(TileType.PILLOW), T(TileType.BOMB), T(TileType.LAMP) },
                { T(TileType.MUG), T(TileType.FLOWER), T(TileType.WREATH) }
            };

            board.ActivateBombAndClear(1, 1);

            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    Assert.Null(board.Board[r, c]);
        }

        [Fact]
        public void Test_CorrectDropTiles()
        {
            // Входные данные:
            // L B C
            // _ W _
            // M F P
            // Ожидаемые: R B R
            //            L W C
            //            M F P

            var board = new Match3BoardService(null, 3, 3);
            board.Board = new Tile[,]
            {
                { T(TileType.LAMP), T(TileType.BRICK), T(TileType.CAT) },
                { null, T(TileType.WREATH), null },
                { T(TileType.MUG), T(TileType.FLOWER), T(TileType.PILLOW) }
            };

            board.DropTiles();

            Assert.Equal(TileType.LAMP, board.Board[1, 0]?.Type);
            Assert.Equal(TileType.WREATH, board.Board[1, 1]?.Type);
            Assert.Equal(TileType.CAT, board.Board[1, 2]?.Type);
        }

        [Fact]
        public void Test_CreateBonusRocketLeftDown()
        {
            // Входные данные:
            // F L M L
            // P L M P
            // L M L L
            // Ожидаемые: F R    R R (R - random, R(b) - rocket)
            //            P R    M L
            //            M R(b) M P

            var board = new Match3BoardService(null, 3, 4);
            board.Board = new Tile[,]
            {
                { T(TileType.FLOWER), T(TileType.LAMP), T(TileType.MUG), T(TileType.LAMP) },
                { T(TileType.PILLOW), T(TileType.LAMP), T(TileType.MUG), T(TileType.PILLOW) },
                { T(TileType.LAMP), T(TileType.MUG), T(TileType.LAMP), T(TileType.LAMP) }
            };

            board.TrySwapAndProcess(2, 0, 2, 1);

            Assert.Equal(TileType.ROCKET, board.Board[2, 1]?.Type);
        }

        [Fact]
        public void Test_CreateBonusRocketLeftUp()
        {
            // Входные данные:
            // L F L L
            // P L M P
            // M L F L
            // Ожидаемые: F R    R R (R - random, R(b) - rocket)
            //            P R    M P
            //            M R(b) F L

            var board = new Match3BoardService(null, 3, 4);
            board.Board = new Tile[,]
            {
                { T(TileType.LAMP), T(TileType.FLOWER), T(TileType.LAMP), T(TileType.LAMP) },
                { T(TileType.PILLOW), T(TileType.LAMP), T(TileType.MUG), T(TileType.PILLOW) },
                { T(TileType.MUG), T(TileType.LAMP), T(TileType.FLOWER), T(TileType.LAMP) }
            };

            board.TrySwapAndProcess(0, 0, 0, 1);

            Assert.Equal(TileType.ROCKET, board.Board[0, 3]?.Type);
        }

        [Fact]
        public void Test_CreateBonusRocketRightUp()
        {
            // Входные данные:
            // L L F L
            // P M L P
            // M F L B
            // Ожидаемые: R R R    F (R - random, R(b) - rocket)
            //            P M R    P
            //            M F R(b) B

            var board = new Match3BoardService(null, 3, 4);
            board.Board = new Tile[,]
            {
                { T(TileType.LAMP), T(TileType.LAMP), T(TileType.FLOWER), T(TileType.LAMP) },
                { T(TileType.PILLOW), T(TileType.MUG), T(TileType.LAMP), T(TileType.PILLOW) },
                { T(TileType.MUG), T(TileType.FLOWER), T(TileType.LAMP), T(TileType.BRICK) }
            };

            board.TrySwapAndProcess(0, 2, 0, 3);

            Assert.Equal(TileType.ROCKET, board.Board[2, 2]?.Type);
        }

        [Fact]
        public void Test_CreateBonusRocketRightDown()
        {
            // Входные данные:
            // F B L B
            // P M L P
            // L L P L
            // Ожидаемые: F    R R B (R - random, R(b) - rocket)
            //            P    B R P
            //            R(b) M R P

            var board = new Match3BoardService(null, 3, 4);
            board.Board = new Tile[,]
            {
                { T(TileType.FLOWER), T(TileType.BRICK), T(TileType.LAMP), T(TileType.BRICK) },
                { T(TileType.PILLOW), T(TileType.MUG), T(TileType.LAMP), T(TileType.PILLOW) },
                { T(TileType.LAMP), T(TileType.LAMP), T(TileType.PILLOW), T(TileType.LAMP) }
            };

            board.TrySwapAndProcess(2, 2, 2, 3);

            Assert.Equal(TileType.ROCKET, board.Board[2, 0]?.Type);
            Assert.Equal(TileType.MUG, board.Board[2, 1]?.Type);
            Assert.Equal(TileType.PILLOW, board.Board[1, 0]?.Type);
        }

        [Fact]
        public void Test_ClearAfterBonusRocket()
        {
            // Входные данные: (R(b) - rocket)
            // F L    M 
            // P R(b) M 
            // L M    L 
            // Ожидаемые: F N M (N - none)
            //            N N N
            //            M N L 

            var board = new Match3BoardService(null, 3, 3);
            board.Board = new Tile[,]
            {
                { T(TileType.FLOWER), T(TileType.LAMP), T(TileType.MUG) },
                { T(TileType.PILLOW), T(TileType.ROCKET), T(TileType.MUG) },
                { T(TileType.LAMP), T(TileType.MUG), T(TileType.LAMP) }
            };

            board.ActivateRocketAndClear(1, 1);

            Assert.Null(board.Board[0, 1]?.Type);
            Assert.Null(board.Board[1, 1]?.Type);
            Assert.Null(board.Board[2, 1]?.Type);
            Assert.Null(board.Board[1, 1]?.Type);
            Assert.Null(board.Board[1, 2]?.Type);
        }

        [Fact]
        public void Test_ExchangeBombsCells()
        {
            // Входные данные: (B - bomb)
            // B L 
            // P F 
            // Ожидаемые: B L
            //            P F 

            var board = new Match3BoardService(null, 2, 2);
            board.Board = new Tile[,]
            {
                { T(TileType.BOMB), T(TileType.LAMP) },
                { T(TileType.PILLOW), T(TileType.FLOWER), }
            };

            board.TrySwapAndProcess(0, 0, 0, 1);

            Assert.Equal(TileType.BOMB, board.Board[0, 0]?.Type);
            Assert.Equal(TileType.LAMP, board.Board[0, 1]?.Type);
            Assert.Equal(TileType.PILLOW, board.Board[1, 0]?.Type);
            Assert.Equal(TileType.FLOWER, board.Board[1, 1]?.Type);
        }

        [Fact]
        public void Test_ExchangeCellsBombs()
        {
            // Входные данные: (B - bomb)
            // B L 
            // P F 
            // Ожидаемые: B L
            //            P F 

            var board = new Match3BoardService(null, 2, 2);
            board.Board = new Tile[,]
            {
                { T(TileType.BOMB), T(TileType.LAMP) },
                { T(TileType.PILLOW), T(TileType.FLOWER), }
            };

            board.TrySwapAndProcess(0, 1, 0, 0);

            Assert.Equal(TileType.BOMB, board.Board[0, 0]?.Type);
            Assert.Equal(TileType.LAMP, board.Board[0, 1]?.Type);
            Assert.Equal(TileType.PILLOW, board.Board[1, 0]?.Type);
            Assert.Equal(TileType.FLOWER, board.Board[1, 1]?.Type);
        }

        [Fact]
        public void Test_ExchangeCellsRockets()
        {
            // Входные данные: (R - rocket)
            // R L 
            // P F 
            // Ожидаемые: R L
            //            P F 

            var board = new Match3BoardService(null, 2, 2);
            board.Board = new Tile[,]
            {
                { T(TileType.ROCKET), T(TileType.LAMP) },
                { T(TileType.PILLOW), T(TileType.FLOWER), }
            };

            board.TrySwapAndProcess(0, 1, 0, 0);

            Assert.Equal(TileType.ROCKET, board.Board[0, 0]?.Type);
            Assert.Equal(TileType.LAMP, board.Board[0, 1]?.Type);
            Assert.Equal(TileType.PILLOW, board.Board[1, 0]?.Type);
            Assert.Equal(TileType.FLOWER, board.Board[1, 1]?.Type);
        }

        [Fact]
        public void Test_ExchangeRocketsCells()
        {
            // Входные данные: (R - rocket)
            // R L 
            // P F 
            // Ожидаемые: R L
            //            P F 

            var board = new Match3BoardService(null, 2, 2);
            board.Board = new Tile[,]
            {
                { T(TileType.ROCKET), T(TileType.LAMP) },
                { T(TileType.PILLOW), T(TileType.FLOWER), }
            };

            board.TrySwapAndProcess(0, 0, 0, 1);

            Assert.Equal(TileType.ROCKET, board.Board[0, 0]?.Type);
            Assert.Equal(TileType.LAMP, board.Board[0, 1]?.Type);
            Assert.Equal(TileType.PILLOW, board.Board[1, 0]?.Type);
            Assert.Equal(TileType.FLOWER, board.Board[1, 1]?.Type);
        }

        [Fact]
        public void Test_ClearAndDropAfterBonusRocket()
        {
            // Входные данные: (R(b) - rocket)
            // F L    M 
            // P R(b) M 
            // L M    L 
            // Ожидаемые: R R R (R - random)
            //            F R M
            //            M R L 

            var board = new Match3BoardService(null, 3, 3);
            board.Board = new Tile[,]
            {
                { T(TileType.FLOWER), T(TileType.LAMP), T(TileType.MUG) },
                { T(TileType.PILLOW), T(TileType.ROCKET), T(TileType.MUG) },
                { T(TileType.LAMP), T(TileType.MUG), T(TileType.LAMP) }
            };

            board.TrySwapAndProcess(1, 1, 1, 1);

            Assert.Equal(TileType.FLOWER, board.Board[1, 0]?.Type);
            Assert.Equal(TileType.MUG, board.Board[1, 2]?.Type);
        }

        private static Tile T(TileType type) => new Tile(type);
    }
}
