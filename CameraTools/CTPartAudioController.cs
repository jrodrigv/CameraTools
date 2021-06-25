using UnityEngine;
namespace CameraTools
{
	public class CTPartAudioController : MonoBehaviour
	{
		Vessel vessel;
		Part part;

		public AudioSource audioSource;


		float origMinDist = 1;
		float origMaxDist = 1;

		float modMinDist = 10;
		float modMaxDist = 10000;

		AudioRolloffMode origRolloffMode;

		void Awake()
		{
			part = GetComponentInParent<Part>();
			vessel = part.vessel;

			CamTools.OnResetCTools += OnResetCTools;
		}

		void Start()
		{
			if (!audioSource)
			{
				Destroy(this);
				return;
			}

			origMinDist = audioSource.minDistance;
			origMaxDist = audioSource.maxDistance;
			origRolloffMode = audioSource.rolloffMode;
			audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
			audioSource.spatialBlend = 1;

		}

		void FixedUpdate()
		{
			if (!audioSource)
			{
				Destroy(this);
				return;
			}

			if (!part || !vessel || !FlightCamera.fetch)
			{
				Destroy(this);
				return;
			}


			float angleToCam = Vector3.Angle(vessel.srf_velocity, FlightCamera.fetch.mainCamera.transform.position - vessel.transform.position);
			angleToCam = Mathf.Clamp(angleToCam, 1, 180);

			float srfSpeed = (float)vessel.srfSpeed;
			srfSpeed = Mathf.Min(srfSpeed, 550f);

			float lagAudioFactor = (75000 / (Vector3.Distance(vessel.transform.position, FlightCamera.fetch.mainCamera.transform.position) * srfSpeed * angleToCam / 90));
			lagAudioFactor = Mathf.Clamp(lagAudioFactor * lagAudioFactor * lagAudioFactor, 0, 4);
			lagAudioFactor += srfSpeed / 230;

			float waveFrontFactor = ((3.67f * angleToCam) / srfSpeed);
			waveFrontFactor = Mathf.Clamp(waveFrontFactor * waveFrontFactor * waveFrontFactor, 0, 2);
			if (vessel.srfSpeed > CamTools.speedOfSound)
			{
				waveFrontFactor = (srfSpeed / (angleToCam) < 3.67f) ? waveFrontFactor + ((srfSpeed / (float)CamTools.speedOfSound) * waveFrontFactor) : 0;
			}

			lagAudioFactor *= waveFrontFactor;

			audioSource.minDistance = Mathf.Lerp(origMinDist, modMinDist * lagAudioFactor, Mathf.Clamp01((float)vessel.srfSpeed / 30));
			audioSource.maxDistance = Mathf.Lerp(origMaxDist, Mathf.Clamp(modMaxDist * lagAudioFactor, audioSource.minDistance, 16000), Mathf.Clamp01((float)vessel.srfSpeed / 30));

		}

		void OnDestroy()
		{
			CamTools.OnResetCTools -= OnResetCTools;


		}

		void OnResetCTools()
		{
			if (audioSource != null)
			{
				audioSource.minDistance = origMinDist;
				audioSource.maxDistance = origMaxDist;
				audioSource.rolloffMode = origRolloffMode;
			}
			Destroy(this);
		}


	}
}

