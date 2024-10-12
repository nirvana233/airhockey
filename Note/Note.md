# Air Hockey (2D 曲棍球)

## URL
https://github.com/nirvana233/airhockey.git

## 描述
一款2D曲棍球物理小游戏，律师题材。画面比较精美，动画做的很好，代码层面东西不算很多很复杂，主要是动画那些比较多。动画有些是自己写的一些插值之类的数学库做的，有些就是Animation做的。

![](airhockey.png)

- 移动是调用的_rigidBody.MovePosition(position);
- 一开始是竖屏，进入游戏后代码调用切换成横屏
- Screen.orientation = ScreenOrientation.LandscapeLeft;
- 输入用的新的输入系统
- 以16:9拼接的UI，比这个还宽的屏幕，会去设置相机orthographicSize大小来控制显示

```csharp
if (screenRatio < _minimumRatio)
{
    _camera.orthographicSize = _minimumRatio / screenRatio * _camera.orthographicSize;
}
```

- UniTask的使用，见代码

``` csharp
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace LazySquirrelLabs.AirHockey.Utils
{
	internal static class UniTaskExtensions
	{
		#region Internal

		/// <summary>
		/// Asynchronously progresses an <paramref name="update"/> function through a range.
		/// </summary>
		/// <param name="update">The function to be invoked during progression.</param>
		/// <param name="start">The start value of the progression.</param>
		/// <param name="end">The end value of the progression.</param>
		/// <param name="duration">The duration of the progression in seconds.</param>
		/// <param name="token">The token used for cancellation.</param>
		/// <returns>The awaitable task.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="duration"/>
		/// is negative.</exception>
		internal static async UniTask ProgressAsync(Action<float> update, float start, float end, float duration,
		                                            CancellationToken token)
		{
			if (duration < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(duration), duration, "Duration must be positive.");
			}
			
			var startTime = Time.time;
			var delta = 0f;

			while (delta <= duration)
			{
				var value = Mathf.Lerp(start, end, delta / duration);
				update(value);
				await UniTask.Yield(PlayerLoopTiming.Update, token);
				token.ThrowIfCancellationRequested();
				delta = Time.time - startTime;
			}

			update(end);
		}

		#endregion
	}
}


using System.Threading;
using Cysharp.Threading.Tasks;
using LazySquirrelLabs.AirHockey.Utils;
using UnityEngine;
using UniTaskExtensions = LazySquirrelLabs.AirHockey.Utils.UniTaskExtensions;

namespace LazySquirrelLabs.AirHockey.UI
{
	/// <summary>
	/// Fades a <see cref="CanvasGroup"/> in out asynchronously.
	/// </summary>
	internal class CanvasFader : MonoBehaviour
	{
		#region Serialized fields

		[SerializeField] private Canvas _canvas;
		[SerializeField] private CanvasGroup _canvasGroup;

		#endregion

		#region Fields

		private readonly CancellationTokenSource _cancellationTokenSource = new();

		#endregion

		#region Setup

		private void OnDestroy()
		{
			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();
		}

		#endregion

		#region Internal

		/// <summary>
		/// Fades the <see cref="Canvas"/> in, asynchronously.
		/// </summary>
		/// <param name="duration">The duration of the fade, in seconds.</param>
		/// <param name="token">Token used for task cancellation.</param>
		/// <returns>The awaitable task.</returns>
		internal async UniTask FadeInAsync(float duration, CancellationToken token)
		{
			var unifiedToken = token.Unify(_cancellationTokenSource.Token);
			_canvas.enabled = true;
			await FadeAsync(0f, 1f, duration, unifiedToken);
		}

		/// <summary>
		/// Fades the <see cref="Canvas"/> out, asynchronously.
		/// </summary>
		/// <param name="duration">The duration of the fade, in seconds.</param>
		/// <param name="token">Token used for task cancellation.</param>
		/// <returns>The awaitable task.</returns>
		internal async UniTask FadeOutAsync(float duration, CancellationToken token)
		{
			var unifiedToken = token.Unify(_cancellationTokenSource.Token);
			await FadeAsync(1f, 0f, duration, unifiedToken);
			_canvas.enabled = false;
		}

		#endregion

		#region Private

		/// <summary>
		/// Fades a <see cref="Canvas"/> asynchronously.
		/// </summary>
		/// <param name="from">The start value of the <see cref="Canvas"/>'s alpha.</param>
		/// <param name="to">The end value of the <see cref="Canvas"/>'s alpha.</param>
		/// <param name="duration">The duration of the fade, in seconds.</param>
		/// <param name="token">Token used for task cancellation.</param>
		/// <returns>The awaitable task.</returns>
		private async UniTask FadeAsync(float from, float to, float duration, CancellationToken token)
		{
			await UniTaskExtensions.ProgressAsync(SetAlpha, from, to, duration, token);
			return;

			void SetAlpha(float alpha) => _canvasGroup.alpha = alpha;
		}

		#endregion
	}
}


using System.Threading;

namespace LazySquirrelLabs.AirHockey.Utils
{
	internal static class CancellationTokenExtensions
	{
		#region Internal

		/// <summary>
		/// Unifies 2 <see cref="CancellationToken"/> into a single one. The unified token will be cancelled whenever
		/// any of the provided tokens gets cancelled.
		/// </summary>
		/// <param name="token1">The first token to be unified.</param>
		/// <param name="token2">The second token to be unified.</param>
		/// <returns>The unified token.</returns>
		internal static CancellationToken Unify(this CancellationToken token1, CancellationToken token2)
		{
			return CancellationTokenSource.CreateLinkedTokenSource(token1, token2).Token;
		}

		#endregion
	}
}

```

