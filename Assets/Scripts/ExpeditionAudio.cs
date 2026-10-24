using UnityEngine;
namespace Aster {
public class ExpeditionAudio:MonoBehaviour {
 AudioSource music,ambience,fx;public float musicVolume=.6f,effectsVolume=.8f;
 public void Setup(){musicVolume=PlayerPrefs.GetFloat("music",.6f);effectsVolume=PlayerPrefs.GetFloat("effects",.8f);music=Source("exploration",true);ambience=Source("forest",true);fx=gameObject.AddComponent<AudioSource>();Apply();}
 AudioSource Source(string name,bool loop){var s=gameObject.AddComponent<AudioSource>();s.clip=Resources.Load<AudioClip>("Audio/"+name);s.loop=loop;s.Play();return s;}
 public void Apply(){if(!music)return;music.volume=musicVolume*.55f;ambience.volume=effectsVolume*.3f;fx.volume=effectsVolume;PlayerPrefs.SetFloat("music",musicVolume);PlayerPrefs.SetFloat("effects",effectsVolume);}
 public void Play(string name,float volume=.75f){var c=Resources.Load<AudioClip>("Audio/"+name);if(c)fx.PlayOneShot(c,volume);}
 void Update(){if(music&&music.clip&&!music.loop&&!music.isPlaying)ResetMusic();}
 public void Ending(){music.clip=Resources.Load<AudioClip>("Audio/ending");music.loop=false;music.Play();}
 public void ResetMusic(){music.clip=Resources.Load<AudioClip>("Audio/exploration");music.loop=true;music.Play();}
 public AudioSource LoopAt(string name,Transform t){var s=t.gameObject.AddComponent<AudioSource>();s.clip=Resources.Load<AudioClip>("Audio/"+name);s.loop=true;s.spatialBlend=1;s.minDistance=2;s.maxDistance=22;s.rolloffMode=AudioRolloffMode.Linear;s.volume=effectsVolume*.45f;return s;}
}
}
