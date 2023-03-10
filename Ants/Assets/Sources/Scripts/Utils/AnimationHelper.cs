using System;
using System.Collections;
using UnityEngine;

namespace App.Core.Utils.Animation
{
	public static class AnimationHelper {

		private static AnimationCurve _easeInOut = AnimationCurve.EaseInOut(0f, 0f,1f,1f);

		public static IEnumerator AnimateWithCurve(float animationTime, Action<float> onProgress, AnimationCurve curve) {
			yield return AnimateInternal(0f, animationTime, onProgress, curve.Evaluate);
		}

		public static IEnumerator AnimateEaseInOut(float animationTime, Action<float> onProgress) {
			yield return AnimateInternal(0f, animationTime, onProgress, _easeInOut.Evaluate);
		}

		public static IEnumerator AnimateEaseInOut(float startTime, float animationTime, Action<float> onProgress) {
			yield return AnimateInternal(startTime, animationTime, onProgress, _easeInOut.Evaluate);
		}

		public static IEnumerator AnimateLinear(float startTime, float animationTime, Action<float> onProgress) {
			yield return AnimateInternal(startTime, animationTime, onProgress, value => value);
		}
		
		public static IEnumerator AnimateLinear(float animationTime, Action<float> onProgress) {
			yield return AnimateLinear(0f, animationTime, onProgress);
		}

		private static IEnumerator AnimateInternal(float startTime, float animationTime, Action<float> onProgress, Func<float, float> progressPrepare) {
			var timer = startTime;
			var progress = progressPrepare.Invoke(timer / animationTime);
			onProgress?.Invoke(progress);
			yield return null;
			while (timer != animationTime)
			{
				timer += Time.deltaTime;
				timer = Mathf.Clamp(timer, 0f, animationTime);
				progress = progressPrepare.Invoke(timer / animationTime);
				onProgress?.Invoke(progress);
				yield return null;
			}
		}


		public static float EvaluateEaseInOut(float progress) {
			return _easeInOut.Evaluate(progress);
		}
		
	}
}