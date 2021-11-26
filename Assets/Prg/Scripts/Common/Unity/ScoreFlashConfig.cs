using System;
using UnityEngine;

namespace Prg.Scripts.Common.Unity
{
    /// <summary>
    /// Configuration for <c>ScoreFlash</c>.
    /// </summary>
    [Serializable]
    public class ScoreFlashPhases
    {
        #region Parameters for the Fade In Phase

        /// <summary>
        ///     The time in seconds that it takes to fade in.
        /// </summary>
        [Header("Fade In")] public float _fadeInTimeSeconds = 0.5f;

        /// <summary>
        ///     Initial color.
        /// </summary>
        public Color _fadeInColor = new Color(0.0f, 1.0f, 0.2f, 0.0f);

        /// <summary>
        ///     The animation curve used to drive the color from colorFadeIn to colorReadStart.
        /// </summary>
        /// <remarks>
        ///     For a <strong>tutorial</strong> on working with colors in ScoreFlash,
        /// </remarks>
        public AnimationCurve _fadeInColorCurve = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>
        ///     The offset the message starts with relative to its main screen position.
        ///     Positive values in X make the message come from right, negative messages
        ///     make the message come from left.
        /// </summary>
        public float _fadeInOffsetX;

        /// <summary>
        ///     The animation curve used to drive fadeInOffset.y while fading in.
        /// </summary>
        public AnimationCurve _fadeInOffsetXCurve = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>
        ///     The offset the message starts with relative to its main screen position.
        ///     Positive values in Y make the message come from below, negative values make
        ///     the message come from above.
        /// </summary>
        public float _fadeInOffsetY = 150.0f;

        /// <summary>
        ///     The animation curve used to drive fadeInOffset.y while fading in.
        /// </summary>
        public AnimationCurve _fadeInOffsetYCurve = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>
        ///     The initial scale when the message appears.
        /// </summary>
        public float _fadeInScale = 0.001f;

        /// <summary>
        ///     The animation curve used to drive the scale from <c>fadeInScale</c>
        ///     to it's read start value (this is always <c>1</c>).
        /// </summary>
        public AnimationCurve _fadeInScaleCurve = AnimationCurve.Linear(0, 0, 1, 1);

        #endregion Parameters for the Fade In Phase

        #region Parameters for the Reading Phase

        /// <summary>
        ///     The time the message is kept for the player to read (max to read),
        /// </summary>
        [Header("Stay")] public float _readTimeSeconds = 2.5f;

        /// <summary>
        ///     Color faded to from initial color in first phase.
        /// </summary>
        public Color _readColorStart = new Color(1.0f, 1.0f, 0.0f, 1.0f);

        /// <summary>
        ///     The animation curve used to drive the color from colorReadStart to colorReadEnd.
        /// </summary>
        public AnimationCurve _readColorCurve = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>
        ///     Color faded to from max color in "read phase".
        /// </summary>
        public Color _readColorEnd = new Color(0.0f, 0.7f, 0.7f, 0.3f);

        /// <summary>
        ///     The animation curve used to drive the velocity on the x-axis from <c>0</c>
        ///     to it's read end value <c>readTimeFloatUpVelocity</c>.
        /// </summary>
        public AnimationCurve _readVelocityXCurve = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>
        ///     Speed with which the message floats right while it fades out. Use negative
        ///     values to make it float left.
        ///     WARNING: Values between than -15 and 15 (but not 0) may generate stuttering!
        /// </summary>
        public float _readFloatRightVelocity;

        /// <summary>
        ///     The animation curve used to drive the velocity on the y-axis from <c>0</c>
        ///     to it's read end value <c>readTimeFloatUpVelocity</c>.
        /// </summary>
        public AnimationCurve _readVelocityCurve = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>
        ///     Speed with which the message floats up while it fades out. Use negative
        ///     values to make it float down.
        /// </summary>
        public float _readFloatUpVelocity = -40.0f;

        /// <summary>
        ///     The animation curve used to drive the scale from <c>1</c>
        ///     to it's read end value <c>readTimeScale</c>.
        /// </summary>
        public AnimationCurve _readScaleCurve = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>
        ///     Scale at the end of the read time. The message scales from 1
        ///     to this value while it is kept to be read.
        /// </summary>
        public float _readScale = 1.5f;

        #endregion Parameters for the Reading Phase

        #region Parameters for the Fade Out Phase

        /// <summary>
        ///     The time the message takes to fade out.
        /// </summary>
        [Header("Fade Out")] public float _fadeOutTimeSeconds = 1.0f;

        /// <summary>
        ///     The animation curve used to drive the color from colorReadEnd to colorFadeOut.
        /// </summary>
        public AnimationCurve _fadeOutColorCurve = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>
        ///     Color faded to from read color in fade out phase. Should
        ///     have alpha = 0!
        ///     WARNING: If "alpha != 0" => text disappears abruptly!
        /// </summary>
        public Color _fadeOutColor = new Color(1.0f, 0.0f, 0.8f, 0.0f);

        /// <summary>
        ///     The animation curve used to drive the velocity on x from <c>readTimeFloatUpVelocity</c>
        ///     to it's fade out end value <c>fadeOutTimeFloatUpVelocity</c>.
        /// </summary>
        public AnimationCurve _fadeOutVelocityXCurve = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>
        ///     Speed with which the message floats right while it fades out. Use negative
        ///     values to make it float left.
        ///     WARNING: Values between -15 and 15 (but not 0) may generate stuttering!
        /// </summary>
        public float _fadeOutFloatRightVelocity;

        /// <summary>
        ///     The animation curve used to drive the velocity on y from <c>readTimeFloatUpVelocity</c>
        ///     to it's fade out end value <c>fadeOutTimeFloatUpVelocity</c>.
        /// </summary>
        public AnimationCurve _fadeOutVelocityCurve = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>
        ///     Speed with which the message floats up while it fades out. Use negative
        ///     values to make it float down.
        /// </summary>
        public float _fadeOutFloatUpVelocity = -80.0f;

        /// <summary>
        ///     The animation curve used to drive the scale from <c>readTimeScale</c>
        ///     to it's fade out end value <c>fadeOutTimeScale</c>.
        /// </summary>
        public AnimationCurve _fadeOutScaleCurve = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>
        ///     Scale at the end of the fade out time. The message scales from midScale
        ///     to this value while it is kept to be read.
        /// </summary>
        public float _fadeOutScale = 2.0f;

        /// <summary>
        ///     This is the initial rotation speed the message gets immediately
        ///     when moving into fading out. Use rotationAcceleration to make
        ///     this a little smoother. Set to 0 to not have the message rotate
        ///     on fading out (rotationAcceleration must also be 0).
        /// </summary>
        public float _fadeOutInitialRotationSpeed;

        /// <summary>
        ///     Increases the rotation speed while the message fades out.
        ///     Set to 0 to not have the message rotate on fading out
        ///     (initialRotationSpeed must also be 0).
        /// </summary>
        public float _fadeOutRotationAcceleration = 30.0f;

        #endregion Parameters for the Fade Out Phase
    }

    [CreateAssetMenu(menuName = "ALT-Zone/ScoreFlashConfig", fileName = "ScoreFlashConfig")]
    public class ScoreFlashConfig : ScriptableObject
    {
        public Canvas _canvasPrefab;
        public ScoreFlashPhases _phases;
    }
}