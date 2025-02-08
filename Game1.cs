using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace MyFirstGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Variabile per le sprite
        Texture2D targetSprite;
        Texture2D crosshairsSprite;
        Texture2D backgroundSprite;
        Texture2D bullet;

        // Variabile per i suoni
        SoundEffect shootSound;
        SoundEffect emptySound;
        SoundEffect reloadSound;
        // musica di sottofondo
        private Song opening;
        private Song loop;
        private bool isLoopPlaying = false;

        // Variabile per i font
        SpriteFont gameFont;

        // variabili per la posizione del target
        Vector2 targetPosition = new Vector2(300, 300);
        const int TARGET_RADIUS = 45; // raggio del target
        const int CROSSHAIRS_RADIUS = 22; // raggio del mirino

        MouseState mState; // stato del mouse
        bool mReleased = true; // se il tasto del mouse e' stato rilasciato
        int score = 0; // punteggio
        int errors = 0; // errori

        // variabili per i proiettili
        List<Vector2> bulletPositions = new List<Vector2>(); // Lista delle posizioni dei proiettili
        int bulletLeft = 6; // proiettili rimanenti

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;

            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            for (int i = 0; i < 6; i++)
            {
                bulletPositions.Add(new Vector2(50 + i * 20, 50)); // Spazio tra i proiettili
            }

            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // caricamento musica di sottofondo
            opening = Content.Load<Song>("Soundtrack/opening");
            loop = Content.Load<Song>("Soundtrack/loop");

            // Assegna l'evento per il cambio di traccia
            MediaPlayer.MediaStateChanged += MediaPlayer_MediaStateChanged;

            // Avvia la prima traccia
            MediaPlayer.Play(opening);
            MediaPlayer.IsRepeating = false; // Suoniamo "opening" solo una volta

            _spriteBatch = new SpriteBatch(GraphicsDevice); // usato per disegnare le texture nel metodo Draw()

            // Assegnazione dei sprite
            targetSprite = Content.Load<Texture2D>("Sprites/target");
            crosshairsSprite = Content.Load<Texture2D>("Sprites/crosshairs");
            bullet = Content.Load<Texture2D>("bullet");
            //backgroundSprite = Content.Load<Texture2D>("sky");
            backgroundSprite = Content.Load<Texture2D>("Backgrounds/back");
            // Assengazione del font
            gameFont = Content.Load<SpriteFont>("Fonts/galleryFont"); // caricamento del font
            // Assegnazione del suono
            shootSound = Content.Load<SoundEffect>("SoundEffects/shotgun"); // caricamento del suono
            reloadSound = Content.Load<SoundEffect>("SoundEffects/popClip"); // caricamento del suono
            emptySound = Content.Load<SoundEffect>("SoundEffects/dryFire"); // caricamento del suono
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // segna la posizione del mouse
            mState = Mouse.GetState();

            // incrementa il punteggio se il target e' stato colpito
            if (mState.LeftButton == ButtonState.Pressed && mReleased)
            {
                if (bulletLeft > 0)
                {
                    shootSound.Play(); // riproduce il suono dello sparo
                    RemoveBullet(); // Rimuove l'ultimo proiettile
                    bulletLeft--; // Decrementa il numero di proiettili rimanenti
                }
                else
                {
                    emptySound.Play();
                }

                float mTargetDistance = Vector2.Distance(targetPosition, mState.Position.ToVector2()); // calcola la distanza tra il target e il mouse

                // se la distanza tra il target e il mouse e' minore del raggio del target, incrementa il punteggio
                if (mTargetDistance < TARGET_RADIUS && bulletLeft > 0)
                {
                    // incrementa il punteggio
                    score++;

                    // cambia la posizione del target
                    Random rand = new Random();
                    targetPosition.X = rand.Next(20,_graphics.PreferredBackBufferWidth); // assegna una posizione x casuale al target
                    targetPosition.Y = rand.Next(20, _graphics.PreferredBackBufferHeight); // assegna una posizione v casuale al target
                }
                else
                {
                    errors++;
                }

                mReleased = false;
            }
            // resetta la variabile mReleased quando il tasto del mouse e' rilasciato
            if (mState.LeftButton == ButtonState.Released)
            {
                mReleased = true;
            }

            if (mState.RightButton == ButtonState.Pressed && mReleased)
            {
                ReloadBullets(); // Ricarica i proiettili
                bulletLeft = 6; // Resetta il numero di proiettili rimanenti
                mReleased = false;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Disegno degli sprite
            _spriteBatch.Begin(); // srpiteBatch e' un oggetto che permette di disegnare e deve essere inizializzato con Begin() e terminato con End() (l'oggetto e' inizilizzato nel metodo LoadContent())
            // Le coordinate per il posizionamento da passare al metodo Vector2 sono (x, y) ovvero (WHIDTH, HEIGHT) oppure (desta/sinistra, sopra/sotto). L'origine e' in alto a sinistra (0, 0), i valori sono in pixel (la y funziona al contrario rispetto al piano cartesiano)
            // L'ordine di disegno e' importante, l disegnato e' il primo visualizzato (quindi se disegno prima lo sfondo e poi il target, il target sara' sopra lo sfondo e viceversa)
            //_spriteBatch.Draw(backgroundSprite, new Vector2(0, 0), Color.White); // disegno dello sfondo (con Color.White si mantiene il colore originale)
            _spriteBatch.Draw(backgroundSprite, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            _spriteBatch.Draw(targetSprite, new Vector2(targetPosition.X - TARGET_RADIUS,targetPosition.Y - TARGET_RADIUS), Color.White); // disegno del target e  prende la posizione targetPosition (corregge la posizione (che di default e' in alto a sinistra) sottraendo il raggio del target in modo da centrare la posizione al centro della sprite)
            _spriteBatch.Draw(crosshairsSprite, new Vector2(Mouse.GetState().X - CROSSHAIRS_RADIUS, Mouse.GetState().Y - CROSSHAIRS_RADIUS), Color.Blue); // disegno del mirino
                                                                                                                                                          //_spriteBatch.Draw(bullet, new Vector2(10, 10), null, Color.White, 0f, Vector2.Zero, .15f, SpriteEffects.None, 0f); // disegno i proiettili


            int screenWidth = GraphicsDevice.Viewport.Width;
            int screenHeight = GraphicsDevice.Viewport.Height;

            // Disegno del testo
            //_spriteBatch.DrawString(gameFont, "Shoot at the target!", new Vector2(50, 10), Color.White); // disegno del testo
            Vector2 textSize = gameFont.MeasureString("Shoot at the target!");
            float centerX = (screenWidth - textSize.X) / 2;
            _spriteBatch.DrawString(gameFont, "Shoot the target", new Vector2(centerX, 25), Color.White);
            //_spriteBatch.DrawString(gameFont, $"Score: {score}", new Vector2(650, 10), Color.White); // disegno il punteggio
            Vector2 scoreSize = gameFont.MeasureString($"Score: {score}");
            float scoreX = screenWidth - scoreSize.X - 20; // 20px di margine dal bordo
            _spriteBatch.DrawString(gameFont, $"Score: {score}", new Vector2(scoreX, 25), Color.White);
            //_spriteBatch.DrawString(gameFont, $"Errors: {errors}", new Vector2(450, 10), Color.White); // disegno il punteggio
            Vector2 errorsSize = gameFont.MeasureString($"Errors: {errors}");
            float errorsX = scoreX - errorsSize.X - 20; // 20px di spazio tra Errors e Score
            _spriteBatch.DrawString(gameFont, $"Errors: {errors}", new Vector2(errorsX, 25), Color.White);
            DrawBullets(_spriteBatch, bullet);

            _spriteBatch.End(); // termina il disegno degli sprite

            base.Draw(gameTime);
        }

        private void MediaPlayer_MediaStateChanged(object sender, EventArgs e)
        {
            // Se "opening" è finito e "loop" non sta ancora suonando, avvialo
            if (MediaPlayer.State == MediaState.Stopped && !isLoopPlaying)
            {
                MediaPlayer.Play(loop);
                MediaPlayer.IsRepeating = true; // "loop" deve essere ripetuto all'infinito
                isLoopPlaying = true;
            }
        }

        private void DrawBullets(SpriteBatch _spriteBatch, Texture2D bullet)
        {
            int screenWidth = GraphicsDevice.Viewport.Width;
            int screenHeight = GraphicsDevice.Viewport.Height;

            float bulletHeight = bullet.Height * 0.15f; // Altezza effettiva del proiettile dopo la scala
            float startX = 20; // Margine sinistro
            float startY = 0; // Stessa altezza del testo

            if (bulletLeft > 0)
            {
                // Disegna i proiettili normali
                for (int i = 0; i < bulletPositions.Count; i++)
                {
                    Vector2 bulletPos = new Vector2(startX + i * 20, startY);
                    _spriteBatch.Draw(bullet, bulletPos, null, Color.White, 0f, Vector2.Zero, 0.15f, SpriteEffects.None, 0f);
                }
            }
            else
            {
                // Se il caricatore è vuoto, mostra "reload
                string emptyText = "RELOAD";
                Vector2 textSize = gameFont.MeasureString(emptyText);
                _spriteBatch.DrawString(gameFont, emptyText, new Vector2(startX, startY + 25), Color.Red);
            }
        }



        private void RemoveBullet()
        {
            if (bulletPositions.Count > 0)
            {
                bulletPositions.RemoveAt(bulletPositions.Count - 1); // Rimuove l'ultimo proiettile
            }
        }

        private void ReloadBullets()
        {
            bulletPositions.Clear(); // Svuota tutti i proiettili
            for (int i = 0; i < 6; i++)
            {
                bulletPositions.Add(new Vector2(50 + i * 20, 50)); // Riaggiunge i proiettili
            }
            reloadSound.Play(); // Riproduce il suono del ricaricamento
        }

    }
}
