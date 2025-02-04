using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;

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
            
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // caricamento musica di sottofondo
            opening = Content.Load<Song>("opening");
            loop = Content.Load<Song>("loop");

            // Assegna l'evento per il cambio di traccia
            MediaPlayer.MediaStateChanged += MediaPlayer_MediaStateChanged;

            // Avvia la prima traccia
            MediaPlayer.Play(opening);
            MediaPlayer.IsRepeating = false; // Suoniamo "opening" solo una volta

            _spriteBatch = new SpriteBatch(GraphicsDevice); // usato per disegnare le texture nel metodo Draw()

            // Assegnazione dei sprite
            targetSprite = Content.Load<Texture2D>("target");
            crosshairsSprite = Content.Load<Texture2D>("crosshairs");
            bullet = Content.Load<Texture2D>("bullet");
            //backgroundSprite = Content.Load<Texture2D>("sky");
            backgroundSprite = Content.Load<Texture2D>("back");
            // Assengazione del font
            gameFont = Content.Load<SpriteFont>("galleryFont"); // caricamento del font
            // Assegnazione del suono
            shootSound = Content.Load<SoundEffect>("shotgun"); // caricamento del suono
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
                shootSound.Play(); // riproduce il suono dello sparo

                float mTargetDistance = Vector2.Distance(targetPosition, mState.Position.ToVector2()); // calcola la distanza tra il target e il mouse

                // se la distanza tra il target e il mouse e' minore del raggio del target, incrementa il punteggio
                if (mTargetDistance < TARGET_RADIUS)
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
            //_spriteBatch.Draw(bullet, new Vector2(Mouse.GetState().X - CROSSHAIRS_RADIUS, Mouse.GetState().Y - CROSSHAIRS_RADIUS), Color.White); // disegno del mirino

            // Disegno del testo
            _spriteBatch.DrawString(gameFont, "Shoot at the target!", new Vector2(50, 10), Color.White); // disegno del testo
            _spriteBatch.DrawString(gameFont, $"Score: {score}", new Vector2(650, 10), Color.White); // disegno il punteggio
            _spriteBatch.DrawString(gameFont, $"Errors: {errors}", new Vector2(450, 10), Color.White); // disegno il punteggio

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
    }
}
