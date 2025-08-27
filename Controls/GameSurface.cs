using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

namespace ProjectPacman.Controls;

 public enum Dir { Left, Right, Up, Down, None }

    public sealed class GameSurface : Control
    {
        // --- Ajustes de tiles/sprites ---
        const int Tile = 24;               // píxeles por celda en pantalla
        const int Fps  = 60;
        const double Speed = 4;            // px/frame; ajusta a gusto

        // --- Sprites ---
        Bitmap? _mazeTile;          // pared
        Bitmap? _dot;               // punto
        Bitmap? _power;             // power pellet
        Bitmap? _pacmanSheet;       // spritesheet pacman

        // Recortes de pacman por dirección (3 frames bocas)
        Rect[] _pacRight = Array.Empty<Rect>();
        Rect[] _pacLeft  = Array.Empty<Rect>();
        Rect[] _pacUp    = Array.Empty<Rect>();
        Rect[] _pacDown  = Array.Empty<Rect>();
        int _pacFrame = 0;
        int _pacTick = 0;

        // --- Estado del juego ---
        public int[,] Map = new int[1,1];  // 0: vacío, 1: pared, 2: dot, 3: power
        public int Cols => Map.GetLength(1);
        public int Rows => Map.GetLength(0);

        Point _pacPosPx = new Point(1 * Tile, 1 * Tile);
        Dir _dir = Dir.None;
        Dir _nextDir = Dir.None;

        readonly DispatcherTimer _timer;

        public GameSurface()
        {
            // loop
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000.0 / Fps) };
            _timer.Tick += (_, __) =>
            {
                Update();
                InvalidateVisual();
            };
            _timer.Start();

            // teclado
            AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);

            // Carga sprites al adjuntarse al árbol visual
            AttachedToVisualTree += (_, __) => LoadSprites();
        }

        void LoadSprites()
        {
            // **Ajusta los nombres/ubicaciones si los tuyos son distintos**
            _mazeTile   = Load("avares://ProjectPacman/Assets/Sprites/tiles/wall.png");
            _dot        = Load("avares://ProjectPacman/Assets/Sprites/tiles/dot.png");
            _power      = Load("avares://ProjectPacman/Assets/Sprites/tiles/power.png");
            _pacmanSheet= Load("avares://ProjectPacman/Assets/Sprites/pacman/pacman_sheet.png");

            // Si tu sheet es 3x4 frames (ancho=3, alto=4), por ejemplo 16x16 cada uno:
            const int fw = 16, fh = 16;
            _pacRight = new[] { new Rect(0,0,fw,fh), new Rect(fw,0,fw,fh), new Rect(2*fw,0,fw,fh) };
            _pacLeft  = new[] { new Rect(0,fh,fw,fh), new Rect(fw,fh,fw,fh), new Rect(2*fw,fh,fw,fh) };
            _pacUp    = new[] { new Rect(0,2*fh,fw,fh), new Rect(fw,2*fh,fw,fh), new Rect(2*fw,2*fh,fw,fh) };
            _pacDown  = new[] { new Rect(0,3*fh,fw,fh), new Rect(fw,3*fh,fw,fh), new Rect(2*fw,3*fh,fw,fh) };

            // ---- Mapa: CONECTA con tu modelo existente ----
            // Aquí puedes traer Level/LevelMap del repositorio:
            // Ejemplo de enganche:
            // var level = new Level( LevelMap.LoadFrom("...") );
            // Map = level.ToIntGrid();  // 0..3 según convención arriba
            //
            // Mientras tanto, dejo un mini mapa de prueba 15x10:
            Map = new int[,]
            {
                // 0 vacio, 1 pared, 2 dot, 3 power
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
                {1,2,2,2,2,2,2,0,2,2,2,2,2,3,1},
                {1,2,1,1,1,2,1,1,1,2,1,1,1,2,1},
                {1,2,2,0,2,2,2,2,2,2,2,0,2,2,1},
                {1,2,1,1,1,2,1,0,1,2,1,1,1,2,1},
                {1,2,2,2,2,2,2,2,2,2,2,2,2,2,1},
                {1,2,1,1,1,2,1,1,1,2,1,1,1,2,1},
                {1,3,2,0,2,2,2,2,2,2,2,0,2,2,1},
                {1,2,2,2,2,2,2,0,2,2,2,2,2,2,1},
                {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            };

            // Centra a Pac-Man en una celda libre
            _pacPosPx = CellCenter(1, 1);
        }

        static Bitmap Load(string uri)
        {
            return new Bitmap(AssetLoader.Open(new Uri(uri)));
        }

        // ------------------- INPUT -------------------
        void OnKeyDown(object? s, KeyEventArgs e)
        {
            if (e.Key == Key.Left)  _nextDir = Dir.Left;
            if (e.Key == Key.Right) _nextDir = Dir.Right;
            if (e.Key == Key.Up)    _nextDir = Dir.Up;
            if (e.Key == Key.Down)  _nextDir = Dir.Down;
        }

        // ------------------- UPDATE -------------------
        void Update()
        {
            if (Cols == 0 || Rows == 0) return;

            // Si podemos girar en la celda, aplicamos _nextDir
            if (IsCenteredOnCell(_pacPosPx))
            {
                if (CanMove(_pacPosPx, _nextDir)) _dir = _nextDir;

                // Comer dots/power
                var (r, c) = CellRC(_pacPosPx);
                if (Map[r, c] == 2) Map[r, c] = 0;              // dot
                else if (Map[r, c] == 3) Map[r, c] = 0;         // power (TODO: activar modo frightened)
            }

            // Mover si no choca pared
            if (CanMove(_pacPosPx, _dir))
                _pacPosPx = Step(_pacPosPx, _dir, Speed);

            // Animación
            _pacTick = (_pacTick + 1) % 8;
            if (_pacTick == 0) _pacFrame = (_pacFrame + 1) % 3;
        }

        // ------------------- RENDER -------------------
        public override void Render(DrawingContext ctx)
        {
            base.Render(ctx);

            if (Cols == 0 || Rows == 0) return;

            // Fondo negro
            ctx.FillRectangle(Brushes.Black, new Rect(Bounds.Size));

            // Dibuja mapa por celdas
            for (int r = 0; r < Rows; r++)
            for (int c = 0; c < Cols; c++)
            {
                var dst = new Rect(c * Tile, r * Tile, Tile, Tile);
                switch (Map[r, c])
                {
                    case 1: // pared
                        if (_mazeTile != null)
                            ctx.DrawImage(_mazeTile, new Rect(0, 0, _mazeTile.PixelSize.Width, _mazeTile.PixelSize.Height), dst);
                        else
                            ctx.FillRectangle(Brushes.Blue, dst);
                        break;
                    case 2: // dot
                        if (_dot != null)
                            ctx.DrawImage(_dot, new Rect(0,0,_dot.PixelSize.Width,_dot.PixelSize.Height),
                                          CenteredRect(dst, Tile/5.0, Tile/5.0));
                        else
                            ctx.FillRectangle(Brushes.Gold, CenteredRect(dst, 4, 4));
                        break;
                    case 3: // power
                        if (_power != null)
                            ctx.DrawImage(_power, new Rect(0,0,_power.PixelSize.Width,_power.PixelSize.Height),
                                          CenteredRect(dst, Tile/2.6, Tile/2.6));
                        else
                            ctx.FillRectangle(Brushes.Gold, CenteredRect(dst, 10, 10));
                        break;
                }
            }

            // Pac-Man
            if (_pacmanSheet != null)
            {
                var src = _dir switch
                {
                    Dir.Left  => _pacLeft[_pacFrame],
                    Dir.Right => _pacRight[_pacFrame],
                    Dir.Up    => _pacUp[_pacFrame],
                    Dir.Down  => _pacDown[_pacFrame],
                    _         => _pacRight[_pacFrame],
                };
                var dst = new Rect(_pacPosPx.X - Tile/2.0, _pacPosPx.Y - Tile/2.0, Tile, Tile);
                ctx.DrawImage(_pacmanSheet, src, dst);
            }
            else
            {
                // Fallback si falta el sheet
                var dst = new Rect(_pacPosPx.X - Tile/2.0, _pacPosPx.Y - Tile/2.0, Tile, Tile);
                ctx.FillRectangle(Brushes.Yellow, dst);
            }
        }

        // ------------------- Helpers de grid -------------------
        static Rect CenteredRect(Rect cell, double w, double h)
        {
            var x = cell.X + (cell.Width - w) / 2;
            var y = cell.Y + (cell.Height - h) / 2;
            return new Rect(x, y, w, h);
        }

        static Point Step(Point p, Dir d, double s) => d switch
        {
            Dir.Left  => new Point(p.X - s, p.Y),
            Dir.Right => new Point(p.X + s, p.Y),
            Dir.Up    => new Point(p.X, p.Y - s),
            Dir.Down  => new Point(p.X, p.Y + s),
            _         => p
        };

        static (int r, int c) CellRC(Point px) => ((int)Math.Round(px.Y / Tile), (int)Math.Round(px.X / Tile));
        static Point CellCenter(int c, int r) => new Point(c * Tile, r * Tile);

        bool IsCenteredOnCell(Point px)
        {
            var cx = (int)Math.Round(px.X / Tile) * Tile;
            var cy = (int)Math.Round(px.Y / Tile) * Tile;
            return Math.Abs(px.X - cx) < 0.01 && Math.Abs(px.Y - cy) < 0.01;
        }

        bool CanMove(Point fromPx, Dir d)
        {
            var next = Step(fromPx, d, Tile/2.0); // mira medio tile adelante
            var r = (int)Math.Round(next.Y / Tile);
            var c = (int)Math.Round(next.X / Tile);
            if (r < 0 || r >= Rows || c < 0 || c >= Cols) return false;
            return Map[r, c] != 1; // 1 = pared
        }
    }