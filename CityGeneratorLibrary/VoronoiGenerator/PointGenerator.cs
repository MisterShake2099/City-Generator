﻿using System;
using System.Collections.Generic;
using Helpers;
using Voronoi;

namespace Points
{
    public enum PointGenerationAlgorithm:int
    {
        
        Simple, //simple point generation
        CityLike, //generates additional points in the center of the generation plane
        Uniform,
        Circle
    }


    public static class PointGenerator
    {

        private static Random _rng;

        public static List<Point> Generate(GenerationSettings settings)
        {
            List<Point> generatedPoints;
            var startX = settings.StartX;
            var startY = settings.StartY;
            var width = settings.Width;
            var length = settings.Length;
            var amount = settings.Amount;

            // Seed random
            var seed = settings.UseSeed ? settings.Seed : DateTime.Now.GetHashCode();
            _rng = new Random(seed);

            //generate points according to chosen algorithm
            switch (settings.PointAlgorithm)
            {
                case PointGenerationAlgorithm.Simple:
                    generatedPoints = SimpleSpread(settings);
                    break;
                case PointGenerationAlgorithm.CityLike:
                    generatedPoints = CityLikeSpread(settings);
                    break;
                case PointGenerationAlgorithm.Uniform:
                    generatedPoints = UniformDistribution(settings);
                    break;
                case PointGenerationAlgorithm.Circle:
                    generatedPoints = CircleDistribution(settings);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //generatedPoints = new List<Point>();

            generatedPoints.FilterDoubleValues();

            if (settings.PointAlgorithm != PointGenerationAlgorithm.Circle)
            {
                //Add Bounds as points
                var bounds = new Point(startX, startY);
                generatedPoints.Add(bounds);
                generatedPoints.Add(new Point(bounds.X + width, bounds.Y));
                generatedPoints.Add(new Point(bounds.X, bounds.Y + length));
                generatedPoints.Add(new Point(bounds.X + width, bounds.Y + length));

                var halfWidth = width/2;
                var halfLength = length/2;

                //Add half bounds as points
                generatedPoints.Add(new Point(bounds.X + halfWidth, bounds.Y));
                generatedPoints.Add(new Point(bounds.X + halfWidth, bounds.Y + length));

                generatedPoints.Add(new Point(bounds.X, bounds.Y + halfLength));
                generatedPoints.Add(new Point(bounds.X + width, bounds.Y + halfLength));
            }


            //restore back to original settings
            settings.StartX = startX;
            settings.StartY = startY;
            settings.Width = width;
            settings.Amount = amount;
            settings.Length = length;
            

            return generatedPoints;

        }

        private static List<Point> SimpleSpread(GenerationSettings settings)
        {
            //Create point list
            var points = new List<Point>();
            
            var startX = (int)settings.StartX;
            var startY = (int)settings.StartY;
            var width  = (int)settings.Width;
            var length = (int)settings.Length;

            //Generate points and add them to the collection
            for (var i = 0; i < settings.Amount; ++i)
            {
                var x = _rng.Next(startX, startX + width);
                var y = _rng.Next(startY, startY + length);

                var point = new Point(x, y);
                points.Add(point);
            }
          
            return points;
        }

        private static List<Point> CityLikeSpread(GenerationSettings settings)
        {
            var startX = (int)settings.StartX;
            var startY = (int)settings.StartY;
            var width  = (int)settings.Width;
            var length = (int)settings.Length;
            
            var offset = 0.20;

            //amount of points will be divided over both generations
            settings.Amount = (int)Math.Floor(settings.Amount * 0.3);

            //generate outer points
            var points = SimpleSpread(settings);
            
            //calculate new start point and bounds
            settings.StartX = startX + (width * offset);
            settings.StartY = startY + (length * offset);

            offset *= 2;

            settings.Width = width * (1-offset);
            settings.Length = length * (1-offset);

            //generate inner points
            settings.Amount = (int)Math.Floor(settings.Amount * 0.7);
            points.AddRange(SimpleSpread(settings));



            return points;
        }

        private static List<Point> UniformDistribution(GenerationSettings settings)
        {
            var points = new List<Point>();

            var width = settings.Width;
            var length = settings.Length;

            int gran = 20000;
            int widthM = 0, lengthM = 0;

            if (width > length)
            {
                lengthM = gran;
                widthM = (int)Math.Floor(gran*width/length);
            }
            else
            {
                widthM = gran;
                lengthM = (int)Math.Floor(gran * length / width);
            }

            for (int i = 0; i < settings.Amount; i++)
            {
                var p = Point.Zero;

                p.X = width*RandomHelper.RandomInt((int)settings.StartX, widthM)/widthM;
                p.Y = length * RandomHelper.RandomInt((int)settings.StartY, lengthM) / lengthM;

                points.Add(p);
            }


            return points;
        }

        private static List<Point> CircleDistribution(GenerationSettings settings)
        {
            var points = new List<Point>();

            for (int i = 0; i < settings.Amount; i++)
            {
                var origin = new Point(settings.Width/2, settings.Length/2);
                var p = MathHelpers.GenerateRandomPointInCircle(origin, settings.CircleRadius);

                points.Add(p);
            }

            return points;
        }
    }
}
