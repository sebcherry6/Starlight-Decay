using ASTEROIDS_2;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using static System.Formats.Asn1.AsnWriter;
using System.Reflection;
using System.Linq;
using System.IO;
using System.ComponentModel.Design;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Asteroids_2;

namespace Starlight_Decay
{
    public enum GameStates
    {
        none,
        titleScreen,
        gameplay,
        paused,
        gameOver
    }
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public List<string> _hiScoreNames = new List<string>();
        public List<int> _hiScoreValues = new List<int>();
        protected string _hiScoreFilePath;
        protected string _hiScoreFolderPath;
        protected int _hiScoreNewIndex;

        public Random _random = new Random();

        public GameStates _currentState;
        public Point _screenSize = new Point(1200, 1200);

        public KeyboardState _currentKeys, _previousKeys;

        public Texture2D _rocksTxr, _particleTxr, _shipTxr;

        public SpriteFont _mainFont, _titleFont, _hudFont;
        public SoundEffect _zapSound, _explodeSound, _startSound,
                           _inviSound;



        public Player _player;
        public Ship _ship;

        public List<Rocks> _rocks = new List<Rocks>();
        public List<Rocks> _newRocks = new List<Rocks>();
        public List<Particles> _particles = new List<Particles>();
        public List<Particles> _newParticles = new List<Particles>();
        public List<ParticleShot> _shots = new List<ParticleShot>();


        public int _minRocks = 8;
        



        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = _screenSize.X;
            _graphics.PreferredBackBufferHeight = _screenSize.Y;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();

            base.Initialize();

            _hiScoreFolderPath = Environment.GetFolderPath
                (Environment.SpecialFolder.LocalApplicationData);
            _hiScoreFolderPath = Path.Combine(_hiScoreFolderPath, "Asteroids");
            _hiScoreFilePath = Path.Combine(_hiScoreFolderPath, "hiScore.sav");

            LoadHiScore();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _shipTxr = Content.Load<Texture2D>("SpriteShip");
            _rocksTxr = Content.Load<Texture2D>("SpriteRocks");
            _particleTxr = Content.Load<Texture2D>("SpriteParticle");
            _mainFont = Content.Load<SpriteFont>("MainFont");
            _titleFont = Content.Load<SpriteFont>("TFont");
            _hudFont = Content.Load<SpriteFont>("hudFont");
            _zapSound = Content.Load<SoundEffect>("laser");
            _explodeSound = Content.Load<SoundEffect>("explosion");
            _startSound = Content.Load<SoundEffect>("start");
            _inviSound = Content.Load<SoundEffect>("invi");// TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            _previousKeys = _currentKeys;
            _currentKeys = Keyboard.GetState();

            switch (_currentState)
            {
                case GameStates.titleScreen:
                    UpdateTitleScreen(gameTime);
                    break;
                case GameStates.gameplay:
                    UpdateGameplay(gameTime);
                    break;
                case GameStates.gameOver:
                    UpdateGameOver(gameTime);
                    break;
                case GameStates.paused:
                    UpdatePaused(gameTime);
                    break;

                default:
                    StartTitleScreen();
                    break;
            }

            _rocks.RemoveAll(eachSprite => eachSprite._dead);
            _particles.RemoveAll(eachSprite => eachSprite._dead);
            _shots.RemoveAll(eachSprite => eachSprite._dead);

            base.Update(gameTime);
        }
        protected void UpdateTitleScreen(GameTime gameTime)
        {
            if (_currentKeys.IsKeyDown(Keys.Escape) && !_previousKeys.IsKeyDown(Keys.Escape))
            {
                Exit();
            }
            if (_currentKeys.IsKeyDown(Keys.Space) && !_previousKeys.IsKeyDown(Keys.Space))
            {
                StartGameplay();
            }
            foreach (Rocks eachRock in _rocks)
            {
                eachRock.UpdateSprite(gameTime);
            }
            foreach (Particles eachParticle in _particles)
            {
                eachParticle.UpdateSprite(gameTime);
            }

            _rocks.AddRange(_newRocks);
            _newRocks.Clear();

        }
        protected void UpdateGameplay(GameTime gameTime)
        {
            if (_currentKeys.IsKeyDown(Keys.Escape) && !_previousKeys.IsKeyDown(Keys.Escape))
            {
                SetPaused(true);
            }

            _player.UpdateSprite(gameTime);
            foreach (Rocks eachRock in _rocks)
            {
                eachRock.UpdateSprite(gameTime);
            }
            foreach (Particles eachParticle in _particles)
            {
                eachParticle.UpdateSprite(gameTime);
            }
            foreach (ParticleShot eachShot in _shots)
            {
                eachShot.UpdateSprite(gameTime);
            }
            if (_rocks.Count < _minRocks)
            {
                _rocks.Add(new Rocks(this, null));
            }
            _particles.AddRange(_newParticles);
            _newParticles.Clear();
            _rocks.AddRange(_newRocks);
            _newRocks.Clear();



        }
        protected void UpdateGameOver(GameTime gameTime)
        {
            if (_currentKeys.IsKeyDown(Keys.Escape) && !_previousKeys.IsKeyDown(Keys.Escape))
            {
                StartTitleScreen();
            }
            foreach (Rocks eachRock in _rocks)
            {
                eachRock.UpdateSprite(gameTime);
            }
            foreach (Particles eachParticle in _particles)
            {
                eachParticle.UpdateSprite(gameTime);
            }
            foreach (ParticleShot eachShot in _shots)
            {
                eachShot.UpdateSprite(gameTime);
            }

            if (_hiScoreNewIndex >= 0)
            {
                for (int i = 65; i <= 90; i++)
                {
                    Keys key = (Keys)i;
                    if (_currentKeys.IsKeyDown(key) && !_previousKeys.IsKeyDown(key))
                    {
                        _hiScoreNames[_hiScoreNewIndex] += key.ToString();
                    }
                }
                if (_currentKeys.IsKeyDown(Keys.Back) && !_previousKeys.IsKeyDown(Keys.Back))
                {
                    _hiScoreNames[_hiScoreNewIndex] = _hiScoreNames[_hiScoreNewIndex].Substring(
                        0, 
                        _hiScoreNames[_hiScoreNewIndex].Length - 1);
                    }
                if ((_currentKeys.IsKeyDown(Keys.Enter) && !_previousKeys.IsKeyDown(Keys.Enter))
                    || _hiScoreNames[_hiScoreNewIndex].Length >= 10)
                {
                    _hiScoreNewIndex = -1;
                    SaveHiScore();
                    }
            }


        }
        protected void UpdatePaused(GameTime gameTime)
        {

            if (_currentKeys.IsKeyDown(Keys.Escape) && !_previousKeys.IsKeyDown(Keys.Escape))
            {
                StartTitleScreen();
            }
            if (_currentKeys.IsKeyDown(Keys.Space) && !_previousKeys.IsKeyDown(Keys.Space))
            {
                SetPaused(false);

            }


        }
        public void StartTitleScreen()
        {
            _currentState = GameStates.titleScreen;
            _startSound.Play();
            _rocks.Clear();
            for (int i = 0; i < _minRocks; i++)
            {
                _rocks.Add(new Rocks(this, null, _random.Next(0, 4)));
            }
            _particles.Clear();
        }
        public void StartGameplay()
        {
            _currentState = GameStates.gameplay;
            _particles.Clear();
            _rocks.Clear();
            _player = new Player(this, _screenSize.ToVector2() / 2.0f, -(float)Math.PI / 2.0f);


            for (int i = 0; i < _minRocks; i++)
            {
                _rocks.Add(new Rocks(this, null, _random.Next(0, 4)));
            }

        }
        public void StartGameOver()
        {
            _currentState = GameStates.gameOver;

            _hiScoreNewIndex = AddHiScore("", _player._score);


        }
        public void SetPaused(bool paused)
        {
            if (_currentState == GameStates.paused && !paused)
            {
                _player._coolDownCounter = -1.0f;
                _currentState = GameStates.gameplay;
            }
            else if (_currentState == GameStates.gameplay && paused)
            {
                _currentState = GameStates.paused;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();

            foreach (Rocks eachRocks in _rocks)
            {
                eachRocks.DrawSprite(_spriteBatch);
            }
            foreach (Particles eachParticle in _particles)
            {
                eachParticle.DrawSprite(_spriteBatch);
            }
            foreach (ParticleShot eachShot in _shots)
            {
                eachShot.DrawSprite(_spriteBatch);
            }

            switch (_currentState)
            {
                case GameStates.titleScreen:
                    DrawTitleScreen(gameTime);
                    break;
                case GameStates.gameplay:
                    DrawGameplay(gameTime);
                    break;
                case GameStates.gameOver:
                    DrawGameOver(gameTime);
                    break;
                case GameStates.paused:
                    DrawPaused(gameTime);
                    break;
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
        protected void DrawTitleScreen(GameTime gameTime)
        {

            Vector2 txtPos;

            txtPos = (_screenSize.ToVector2() / 4.0f) - (_titleFont.MeasureString("A S T E R O I D S") / 4.5f);
            _spriteBatch.DrawString(_titleFont, "A S T E R O I D S", txtPos, Color.GreenYellow);

            txtPos = new Vector2((float)_screenSize.X / 2.0f, (float)_screenSize.Y / 10.0f * 6.0f) -
                    (_mainFont.MeasureString("press SPACE to START") / 2.0f);
            _spriteBatch.DrawString(_mainFont, "press SPACE to START", txtPos, Color.GreenYellow);

            txtPos = new Vector2((float)_screenSize.X / 2.0f, (float)_screenSize.Y / 10.0f * 7.0f) -
                (_mainFont.MeasureString("press ESCAPE to EXIT") / 2.0f);
            _spriteBatch.DrawString(_mainFont, "press ESCAPE to EXIT", txtPos, Color.GreenYellow);



        }
        protected void DrawGameplay(GameTime gameTime)
        {
            _player.DrawSprite(_spriteBatch);

            _spriteBatch.DrawString(_hudFont, _player._score.ToString(),
                new Vector2(10.0f, 10.0f), Color.DarkTurquoise);

            Vector2 txtPos;

            txtPos = new Vector2((float)_screenSize.X -
                _hudFont.MeasureString(_player.GetLivesString()).X - 10.0f, 10.0f);

            _spriteBatch.DrawString(_hudFont, _player.GetLivesString(), txtPos, Color.DarkTurquoise);
        }
        protected void DrawGameOver(GameTime gameTime)
        {
            Vector2 txtPos;
            txtPos = (_screenSize.X * new Vector2(0.5f, 0.25f)) - (_titleFont.MeasureString("You DIED!") / 2.0f);
            _spriteBatch.DrawString(_titleFont, "You DIED!", txtPos, Color.Red);

            Color rowColor;
            string rowName;
            for (int i = 0; i < Math.Min(_hiScoreNames.Count, 5); i++)
            {
                if (i == _hiScoreNewIndex)
                {
                    rowColor = Color.Turquoise;

                    if (Math.Floor(gameTime.TotalGameTime.TotalSeconds * 2) % 2 == 0) rowName = _hiScoreNames[i] + "_";
                    else rowName = _hiScoreNames[i]; 
                }
                else
                {
                    rowColor = Color.Red;
                    rowName = _hiScoreNames[i];
                }

                txtPos = new Vector2(_screenSize.X / 4f, (_screenSize.Y / 3f) + (i * 100f));
                _spriteBatch.DrawString(_mainFont, _hiScoreNames[i], txtPos, Color.Turquoise);
       
                txtPos = new Vector2((_screenSize.X / 4f * 3f) - _mainFont.MeasureString(_hiScoreValues[i].ToString()).X,
                     (_screenSize.Y / 3f) + (i * 100f));
                _spriteBatch.DrawString(_mainFont, _hiScoreValues[i].ToString(), txtPos, Color.Turquoise);
            }
            if (_hiScoreNewIndex == -1)
            {
                txtPos = new Vector2((float)_screenSize.X / 2.0f, (float)_screenSize.Y / 10.0f * 9.0f) -
                    (_mainFont.MeasureString("press ESCAPE to return to TITLE SCREEN") / 2.0f);
                _spriteBatch.DrawString(_mainFont, "press ESCAPE to return to TITLE SCREEN", txtPos, Color.GreenYellow);
            }
            txtPos = new Vector2((float)_screenSize.X / 2.0f, (float)_screenSize.Y / 10.0f * 8.0f) -
    (_mainFont.MeasureString("Your score is: " + _player._score) / 2.0f);
            _spriteBatch.DrawString(_mainFont, "Your score is: " + _player._score, txtPos, Color.Red);

        }
        protected void DrawPaused(GameTime gameTime)
        {

            _player.DrawSprite(_spriteBatch);

            Vector2 txtPos;

            txtPos = (_screenSize.ToVector2() / 2.0f) - (_titleFont.MeasureString("PAUSED") / 2.0f);
            _spriteBatch.DrawString(_titleFont, "PAUSED", txtPos, Color.GreenYellow);

            txtPos = new Vector2((float)_screenSize.X / 2.0f, (float)_screenSize.Y / 10.0f * 6.0f) -
        (_mainFont.MeasureString("press SPACE to RESUME") / 2.0f);
            _spriteBatch.DrawString(_mainFont, "press SPACE to RESUME", txtPos, Color.GreenYellow);

            txtPos = new Vector2((float)_screenSize.X / 2.0f, (float)_screenSize.Y / 10.0f * 7.0f) -
                (_mainFont.MeasureString("press ESCAPE to EXIT") / 2.0f);
            _spriteBatch.DrawString(_mainFont, "press ESCAPE to EXIT", txtPos, Color.GreenYellow);

        }
        public void LoadHiScore()
        {
            if (!Directory.Exists(_hiScoreFolderPath))
            {
                Directory.CreateDirectory(_hiScoreFolderPath);
            }
            if (!File.Exists(_hiScoreFilePath))
            {
                File.WriteAllText(_hiScoreFilePath, "");
            }
            string[] loaded = new string[10];
            loaded = File.ReadAllLines(_hiScoreFilePath);

            _hiScoreNames.Clear();
            _hiScoreValues.Clear();

            for (int i = 0; i < loaded.Length - 1; i += 2)
            {
                if (loaded[i] != "" && loaded[i + 1] != "")
                {
                    _hiScoreNames.Add(loaded[i]);
                    _hiScoreValues.Add(int.Parse(loaded[i + 1]));
                }
            }
        }
        public void SaveHiScore()
        {

            string[] loaded = new string[_hiScoreNames.Count * 2];

            for (int i = 0; i < _hiScoreNames.Count; i++)
            {
                loaded[i * 2] = _hiScoreNames[i];
                loaded[(i * 2) + 1] = _hiScoreValues[i].ToString();
            }
            File.WriteAllLines(_hiScoreFilePath, loaded);
        }
        public int AddHiScore(string name, int score)
        {

            int scoreIndex = _hiScoreNames.Count;

            if (_hiScoreNames.Count > 0)
            {
                scoreIndex = -1;
                for (int i = _hiScoreNames.Count - 1; i >= 0; i--)
                {
                    if (score > _hiScoreValues[i])
                    {
                        scoreIndex = i;
                    }

                    else break;
                }
            }
        
                if (scoreIndex >= 0)
                {
                    _hiScoreNames.Insert(scoreIndex, name);
                    _hiScoreValues.Insert(scoreIndex, score);
                while (_hiScoreNames.Count > 5)
                {
                    _hiScoreNames.RemoveAt(_hiScoreNames.Count - 1);
                    _hiScoreValues.RemoveAt(_hiScoreValues.Count - 1);
                }

                SaveHiScore();
                }
                return scoreIndex;

            }

        }
    }


