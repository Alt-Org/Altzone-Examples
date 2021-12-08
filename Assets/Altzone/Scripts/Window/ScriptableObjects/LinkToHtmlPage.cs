﻿using UnityEngine;

namespace Altzone.Scripts.Window.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ALT-Zone/LinkToHtmlPage", fileName = "link-")]
    public class LinkToHtmlPage : ScriptableObject
    {
        [SerializeField] private string _theUrlToUse;
        [SerializeField] private string _theTextToShow;

        public string URL => _theUrlToUse;
        public string Text => _theTextToShow;
    }
}