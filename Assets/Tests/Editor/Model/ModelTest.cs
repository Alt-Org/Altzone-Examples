using Examples.Model.Scripts.Model;
using NUnit.Framework;

namespace Tests.Editor.Model
{
    [TestFixture]
    public class ModelTest
    {
        private const string DefenceNameConfluence = "Confluence";
        private const string ModelNameVitsiniekka = "Vitsiniekka";
        private const string ModelNameTaiteilija = "Taiteilija";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Debug.Log("OneTimeSetUp start");
            ModelLoader.LoadAndClearModels();
            Debug.Log("OneTimeSetUp done");
        }

        [Test]
        public void GetByName()
        {
            var model1 = Models.GetByName<DefenceModel>(DefenceNameConfluence);
            Assert.That(model1, Is.Not.Null);
            var model2 = Models.GetByName<CharacterModel>(ModelNameVitsiniekka);
            Assert.That(model2, Is.Not.Null);
            var model3 = Models.GetByName<CharacterModel>(ModelNameTaiteilija);
            Assert.That(model3, Is.Not.Null);
        }

        [Test]
        public void FindById()
        {
            var model1 = Models.FindById<DefenceModel>(1);
            Assert.That(model1.Id, Is.EqualTo(1));
            var model2 = Models.FindById<CharacterModel>(2);
            Assert.That(model2.Id, Is.EqualTo(2));
            var model3 = Models.FindById<CharacterModel>(3);
            Assert.That(model3.Id, Is.EqualTo(3));
        }

        [Test]
        public void Find_BySelector()
        {
            var model1 = Models.Find<DefenceModel>(x => x.Defence == Defence.Egotism);
            Assert.That(model1.Id, Is.EqualTo((int)Defence.Egotism));
            var model2 = Models.Find<CharacterModel>(x => x.Speed == 6);
            Assert.That(model2.Speed, Is.EqualTo(6));
            var model3 = Models.Find<CharacterModel>(x => x.Name == ModelNameTaiteilija);
            Assert.That(model3.Name, Is.EqualTo(ModelNameTaiteilija));
        }

        [Test]
        public void GetAll()
        {
            var characterModels = Models.GetAll<CharacterModel>();
            const int expectedCharacters = 7;
            Assert.That(characterModels.Count, Is.EqualTo(expectedCharacters));
            var defenceModels = Models.GetAll<DefenceModel>();
            const int expectedDefences = 7;
            Assert.That(defenceModels.Count, Is.EqualTo(expectedDefences));
        }
    }
}