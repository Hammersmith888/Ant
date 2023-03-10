using System.Collections;
//using Google.Play.Review;


/*
	https://forum.unity.com/threads/when-google-play-in-app-review-api-will-be-supported.951822/
	https://github.com/googlesamples/google-signin-unity/issues/133
	https://github.com/googlesamples/google-signin-unity/issues/133
*/


public class GooglePlayReview : MB_Singleton< GooglePlayReview >
{
	// ReviewManager		_reviewManager;
	// PlayReviewInfo		_playReviewInfo;
	//
	//
	// void Start()
	// {
	// 	_reviewManager		= new ReviewManager();
	// }
	//
	//
	// public void Review()
	// {
	// 	StartCoroutine( RequestFlowOperationRoutine() );
	// }
	//
	//
	// IEnumerator RequestFlowOperationRoutine()
	// {
	// 	var requestFlowOperation			= _reviewManager.RequestReviewFlow();
	// 	yield return requestFlowOperation;
	// 	if (requestFlowOperation.Error != ReviewErrorCode.NoError)
	// 	{
	// 	    // Log error. For example, using requestFlowOperation.Error.ToString().
	// 	    yield break;
	// 	}
	// 	_playReviewInfo						= requestFlowOperation.GetResult();
	//
	// 	StartCoroutine( LaunchFlowOperationRoutine() );
	// }
	//
	//
	// IEnumerator LaunchFlowOperationRoutine()
	// {
	// 	var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
	// 	yield return launchFlowOperation;
	// 	_playReviewInfo = null; // Reset the object
	// 	if (launchFlowOperation.Error != ReviewErrorCode.NoError)
	// 	{
	// 	    // Log error. For example, using requestFlowOperation.Error.ToString().
	// 	    yield break;
	// 	}
	// 	// The flow has finished. The API does not indicate whether the user
	// 	// reviewed or not, or even whether the review dialog was shown. Thus, no
	// 	// matter the result, we continue our app flow.
	// }
}

