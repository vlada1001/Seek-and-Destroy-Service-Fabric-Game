using Common;
using System;
using static Common.Library;

namespace PlayerCollection.Model
{
    public class Player
    {
        private Guid id = Guid.NewGuid();
        private string _username;
        private Coord _coordinates;
        private int _hp;
        private int _ad;
        private int _numberOfFights;
        private Status _state;
        private Player _opponent;

        public Guid Id
        {
            get => id;
            set => id = value;
        }

        public string Username
        {
            get => _username;
            set => _username = value;
        }

        public Coord Coordinates
        {
            get => _coordinates;
            set => _coordinates = value;
        }

        public int HP
        {
            get => _hp;
            set => _hp = value;
        }

        public int AD
        {
            get => _ad;
            set => _ad = value;
        }

        public int NumberOfFights
        {
            get => _numberOfFights;
            set => _numberOfFights = value;
        }

        public Status State
        {
            get => _state;
            set => _state = value;
        }

        public Player Opponent
        {
            get => _opponent;
            set
            {
                _opponent = value;
                if (value != null)
                    _state = Status.InFight;
                else
                    _state = Status.Exploring;
            }
        }

        public bool CanFight => _state == Status.Exploring;

        public Player Move()
        {
            Random random = new Random();
            double teleportProb = random.NextDouble();

            if (teleportProb >= 0.9)
            {
                _coordinates.X = random.Next(-100, 100);
                _coordinates.Y = random.Next(-100, 100);
                return this;
            }

            if (State.Equals(Status.Exploring))
            {
                int xJumpLength = random.Next(5, 20);
                int yJumpLength = random.Next(5, 20);

                if (random.Next() % 2 == 0 && Coordinates.X + xJumpLength <= 100)
                    _coordinates.X += xJumpLength;
                else if (random.Next() % 2 == 0 && _coordinates.X - xJumpLength >= -100)
                    _coordinates.X -= xJumpLength;
                if (random.Next() % 2 == 0 && _coordinates.Y + yJumpLength <= 100)
                    _coordinates.Y += yJumpLength;
                else if (random.Next() % 2 == 0 && _coordinates.Y - yJumpLength >= -100)
                    _coordinates.Y -= yJumpLength;
            }

            return this;
        }

        public Player FightPlayer(Player opponent)
        {
            Opponent = opponent;
            return this;
        }

        public Player Init()
        {
            Id = Guid.NewGuid();
            Username = Faker.Internet.UserName();
            HP = 100;
            AD = 20;
            NumberOfFights = 0;
            Coordinates = Coord.Init();
            State = Status.Exploring;

            return this;
        }

        public override bool Equals(object obj)
        {
            return Id == (obj as Player).Id;
        }
    }
}
