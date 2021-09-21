﻿using System;
using System.Collections.Generic;
using System.Net.Http;

namespace YoutubeTranscriptApi
{
    // https://github.com/jdepoix/youtube-transcript-api/blob/master/youtube_transcript_api/_api.py

    /// <summary>
    /// 
    /// </summary>
    public sealed class YouTubeTranscriptApi : IDisposable
    {
        private readonly HttpClient _http_client;

        /// <summary>
        /// Initializes a new instance of the <see cref="YouTubeTranscriptApi"/> class.
        /// </summary>
        public YouTubeTranscriptApi()
        {
            _http_client = new HttpClient();
        }

        /// <summary>
        ///      Retrieves the list of transcripts which are available for a given video. It returns a `TranscriptList` object
        ///        which is iterable and provides methods to filter the list of transcripts for specific languages. While iterating
        ///        over the `TranscriptList` the individual transcripts are represented by `Transcript` objects, which provide
        ///        metadata and can either be fetched by calling `transcript.fetch()` or translated by calling
        ///        `transcript.translate('en')`. Example::
        ///            # retrieve the available transcripts
        ///            transcript_list = YouTubeTranscriptApi.get('video_id')
        ///            # iterate over all available transcripts
        ///            for transcript in transcript_list:
        ///                # the Transcript object provides metadata properties
        ///                print(
        ///                    transcript.video_id,
        ///                    transcript.language,
        ///                    transcript.language_code,
        ///                    # whether it has been manually created or generated by YouTube
        ///                    transcript.is_generated,
        ///                    # a list of languages the transcript can be translated to
        ///                    transcript.translation_languages,
        ///                )
        ///                # fetch the actual transcript data
        ///                print(transcript.fetch())
        ///                # translating the transcript will return another transcript object
        ///                print(transcript.translate('en').fetch())
        ///            # you can also directly filter for the language you are looking for, using the transcript list
        ///            transcript = transcript_list.find_transcript(['de', 'en'])
        ///            # or just filter for manually created transcripts
        ///            transcript = transcript_list.find_manually_created_transcript(['de', 'en'])
        ///            # or automatically generated ones
        ///            transcript = transcript_list.find_generated_transcript(['de', 'en'])
        /// </summary>
        /// <param name="video_id">the youtube video id</param>
        /// <param name="proxies">a dictionary mapping of http and https proxies to be used for the network requests</param>
        /// <param name="cookies">a string of the path to a text file containing youtube authorization cookies</param>
        /// <returns>the list of available transcripts</returns>
        public TranscriptList list_transcripts(string video_id, Dictionary<string, string> proxies = null, string cookies = null)
        {
            if (cookies != null)
            {
                throw new NotImplementedException();
            }

            //    http_client.proxies = proxies if proxies else {}
            return new TranscriptListFetcher(_http_client).fetch(video_id);
        }

        /// <summary>
        /// Retrieves the transcripts for a list of videos.
        /// </summary>
        /// <param name="video_ids">a list of youtube video ids</param>
        /// <param name="languages">A list of language codes in a descending priority. For example, if this is set to ['de', 'en']
        /// it will first try to fetch the german transcript(de) and then fetch the english transcript(en) if it fails to
        /// do so.</param>
        /// <param name="continue_after_error">if this is set the execution won't be stopped, if an error occurs while retrieving
        /// one of the video transcripts</param>
        /// <param name="proxies">a dictionary mapping of http and https proxies to be used for the network requests</param>
        /// <param name="cookies">a string of the path to a text file containing youtube authorization cookies</param>
        /// <returns>a tuple containing a dictionary mapping video ids onto their corresponding transcripts, and a list of
        /// video ids, which could not be retrieved</returns>
        public (Dictionary<string, IEnumerable<TranscriptItem>>, List<string>) get_transcripts(List<string> video_ids, List<string> languages = null, bool continue_after_error = false, Dictionary<string, string> proxies = null, string cookies = null)
        {
            //     :param proxies: a dictionary mapping of http and https proxies to be used for the network requests
            //     :type proxies: { 'http': str, 'https': str} -http://docs.python-requests.org/en/master/user/advanced/#proxies
            if (languages == null)
            {
                languages = new List<string>() { "en" };
            }

            var data = new Dictionary<string, IEnumerable<TranscriptItem>>();
            var unretrievable_videos = new List<string>();

            foreach (var video_id in video_ids)
            {
                try
                {
                    data[video_id] = get_transcript(video_id, languages, proxies, cookies);
                }
                catch (Exception)
                {
                    if (!continue_after_error) throw;
                    unretrievable_videos.Add(video_id);
                }
            }

            return (data, unretrievable_videos);
        }

        /// <summary>
        /// Retrieves the transcript for a single video. This is just a shortcut for calling::
        ///<para>YouTubeTranscriptApi.list_transcripts(video_id, proxies).find_transcript(languages).fetch()</para>
        /// </summary>
        /// <param name="video_id">the youtube video id</param>
        /// <param name="languages">A list of language codes in a descending priority. For example, if this is set to ['de', 'en']
        /// it will first try to fetch the german transcript(de) and then fetch the english transcript(en) if it fails to
        /// do so.</param>
        /// <param name="proxies">a dictionary mapping of http and https proxies to be used for the network requests</param>
        /// <param name="cookies"> a string of the path to a text file containing youtube authorization cookies</param>
        /// <returns></returns>
        public IEnumerable<TranscriptItem> get_transcript(string video_id, IReadOnlyList<string> languages = null, Dictionary<string, string> proxies = null, string cookies = null)
        {
            if (languages == null)
            {
                languages = new List<string>() { "en" };
            }
            //:type proxies: {'http': str, 'https': str} - http://docs.python-requests.org/en/master/user/advanced/#proxies

            return list_transcripts(video_id, proxies, cookies).find_transcript(languages).fetch();
        }

        private void _load_cookies(string cookies, string video_id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _http_client.Dispose();
        }
    }
}
