﻿using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
	public class ObjectComparison
	{
		private Person actualTsar;
		private Person expectedTsar;

		[SetUp]
		public void Setup()
		{
			actualTsar = TsarRegistry.GetCurrentTsar();
			expectedTsar = CreateDefaultTsar();
		}
		
		[Test]
		[Description("Проверка текущего царя")]
		[Category("ToRefactor")]
		public void CheckCurrentTsar()
		{
			actualTsar.Name.Should().Be(expectedTsar.Name);
			actualTsar.Age.Should().Be(expectedTsar.Age);
			actualTsar.Height.Should().Be(expectedTsar.Height);
			actualTsar.Weight.Should().Be(expectedTsar.Weight);

			actualTsar.Parent!.Name.Should().Be(expectedTsar.Parent!.Name);
			actualTsar.Parent.Age.Should().Be(expectedTsar.Parent.Age);
			actualTsar.Parent.Height.Should().Be(expectedTsar.Parent.Height);
			actualTsar.Parent.Parent.Should().Be(expectedTsar.Parent.Parent);
		}

		[Test]
		[Description("Альтернативное решение. Какие у него недостатки?")]
		public void CheckCurrentTsar_WithCustomEquality()
		{
			// Какие недостатки у такого подхода? 
			// При добавлении нового поля в класс Person, нам придется залезать и править функцию AreEqual
			Assert.True(AreEqual(actualTsar, expectedTsar));
		}

		private static bool AreEqual(Person? actual, Person? expected)
		{
			if (actual == expected) return true;
			if (actual == null || expected == null) return false;
			return
				actual.Name == expected.Name
				&& actual.Age == expected.Age
				&& actual.Height == expected.Height
				&& actual.Weight == expected.Weight
				&& AreEqual(actual.Parent, expected.Parent);
		}
		
		private static Person CreateDefaultTsar() 
			=> new Person("Ivan IV The Terrible", 54, 170, 70, CreateDefaultParent());

		private static Person CreateDefaultParent() 
			=> new Person("Vasili III of Russia", 28, 170, 60, null);
	}

	public class TsarRegistry
	{
		public static Person GetCurrentTsar()
		{
			return new Person(
				"Ivan IV The Terrible", 54, 170, 70,
				new Person("Vasili III of Russia", 28, 170, 60, null));
		}
	}

	public class Person
	{
		public static int IdCounter = 0;
		public int Age, Height, Weight;
		public string Name;
		public Person? Parent;
		public int Id;

		public Person(string name, int age, int height, int weight, Person? parent)
		{
			Id = IdCounter++;
			Name = name;
			Age = age;
			Height = height;
			Weight = weight;
			Parent = parent;
		}
	}
}