using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 开场过渡：黑底，正文按「分段」逐段淡入（类似 PPT），结束后进入下一场景。
/// 按住空格达到 <see cref="spaceHoldToSkipSeconds"/> 秒后跳过。
/// </summary>
[DisallowMultipleComponent]
public class OpeningPrologueController : MonoBehaviour
{
    [Header("Next scene (must be in Build Settings)")]
    [SerializeField] private string nextSceneName = "SampleScene";

    [Header("Prologue (segments)")]
    [Tooltip("每一段会单独淡入显示；留空则使用下方内置英文稿。")]
    [SerializeField] private string[] prologueSegments;

    [Header("Segment pacing (seconds, unscaled)")]
    [SerializeField] private float initialBlackDuration = 0.6f;
    [SerializeField] private float segmentFadeInDuration = 1.35f;
    [SerializeField] private float segmentPauseAfterReveal = 3.2f;
    [SerializeField] private float textFadeOutDuration = 1.1f;

    [Header("Skip (hold Space)")]
    [Tooltip("按住空格累计达到该秒数后，跳过开场并进入下一场景。")]
    [SerializeField] private float spaceHoldToSkipSeconds = 0.4f;

    [SerializeField] private bool showSkipHint = true;

    [Tooltip("底部跳过提示（英文默认可避免 LiberationSans 缺字）。")]
    [SerializeField] private string skipHintText = "Hold Space to skip";

    [Header("Fonts (TMP)")]
    [Tooltip("正文为英文时通常可不填；若改用中文分段，请拖入含汉字的 TMP Font Asset。")]
    [SerializeField] private TMP_FontAsset fontWithCjkGlyphs;

    [Header("Typography")]
    [SerializeField] private float bodyFontSize = 30f;
    [SerializeField] private float hintFontSize = 22f;

    private CanvasGroup _segmentsRootGroup;
    private readonly List<CanvasGroup> _segmentGroups = new List<CanvasGroup>();
    private Coroutine _sequenceRoutine;
    private bool _isLoading;

    /// <summary>
    /// 默认英文译本（与中文原稿分段一一对应）。
    /// 中文参考：人说一点点长大 / 成长像遗忘、大雨与黑暗、日出前一小时 / 荆棘与规则、坠落与远行 / 镜中重逢 / 追逐日出。
    /// </summary>
    private static readonly string[] DefaultPrologueEnglish = new[]
    {
        "They say we grow up one small step at a time",
        "But for me, growing older has felt like a long forgetting",
        "I forgot how to run through the rain, how to lift my eyes in the dark, and the self that once followed me like a shadow, most true of all",
        "I locked myself in the last hour before dawn",
        "Here the thorns run deep, the rules lie cold",
        "I must fall—to touch the wounds I dared not face; I must journey outward—to push past the borders I drew around myself",
        "Until the light returns, until I meet again in the mirror that long-lost friend.",
        "This time we will not run. We will chase the sunrise."
    };

    private void Awake()
    {
        BuildUi();
    }

    private void Start()
    {
        _sequenceRoutine = StartCoroutine(PlaySequence());
    }

    private void Update()
    {
        if (_isLoading)
        {
            return;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            _holdSpaceTimer += Time.unscaledDeltaTime;
            if (_holdSpaceTimer >= spaceHoldToSkipSeconds)
            {
                LoadNextScene();
            }
        }
        else
        {
            _holdSpaceTimer = 0f;
        }
    }

    private float _holdSpaceTimer;

    private string[] ResolveSegments()
    {
        if (prologueSegments != null && prologueSegments.Length > 0)
        {
            return prologueSegments;
        }

        return DefaultPrologueEnglish;
    }

    private IEnumerator PlaySequence()
    {
        if (_segmentsRootGroup != null)
        {
            _segmentsRootGroup.alpha = 1f;
        }

        foreach (var cg in _segmentGroups)
        {
            if (cg != null)
            {
                cg.alpha = 0f;
            }
        }

        yield return new WaitForSecondsRealtime(initialBlackDuration);

        if (_segmentGroups.Count == 0)
        {
            LoadNextScene();
            yield break;
        }

        for (var i = 0; i < _segmentGroups.Count; i++)
        {
            if (_isLoading)
            {
                yield break;
            }

            var cg = _segmentGroups[i];
            if (cg == null)
            {
                continue;
            }

            var t = 0f;
            while (t < segmentFadeInDuration)
            {
                if (_isLoading)
                {
                    yield break;
                }

                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Clamp01(t / segmentFadeInDuration);
                yield return null;
            }

            cg.alpha = 1f;
            yield return new WaitForSecondsRealtime(segmentPauseAfterReveal);
        }

        if (_isLoading)
        {
            yield break;
        }

        if (_segmentsRootGroup != null)
        {
            var t = 0f;
            while (t < textFadeOutDuration)
            {
                if (_isLoading)
                {
                    yield break;
                }

                t += Time.unscaledDeltaTime;
                _segmentsRootGroup.alpha = 1f - Mathf.Clamp01(t / textFadeOutDuration);
                yield return null;
            }

            _segmentsRootGroup.alpha = 0f;
        }

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;
        if (_sequenceRoutine != null)
        {
            StopCoroutine(_sequenceRoutine);
            _sequenceRoutine = null;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private void BuildUi()
    {
        var segments = ResolveSegments();

        var canvasGo = new GameObject("PrologueCanvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(canvasGo.transform, false);
        var bgRect = bgGo.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        var bgImage = bgGo.AddComponent<Image>();
        bgImage.color = Color.black;

        var rootGo = new GameObject("SegmentsRoot");
        rootGo.transform.SetParent(canvasGo.transform, false);
        var rootRect = rootGo.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.06f, 0.14f);
        rootRect.anchorMax = new Vector2(0.94f, 0.9f);
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        var vlg = rootGo.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 26f;
        vlg.padding = new RectOffset(12, 12, 8, 8);
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        _segmentsRootGroup = rootGo.AddComponent<CanvasGroup>();
        _segmentsRootGroup.alpha = 1f;
        _segmentsRootGroup.blocksRaycasts = false;
        _segmentsRootGroup.interactable = false;

        var estimatedTextWidth = scaler.referenceResolution.x * (0.94f - 0.06f) - 24f;

        _segmentGroups.Clear();
        foreach (var segment in segments)
        {
            if (string.IsNullOrWhiteSpace(segment))
            {
                continue;
            }

            var segGo = new GameObject("Segment");
            segGo.transform.SetParent(rootGo.transform, false);

            var le = segGo.AddComponent<LayoutElement>();
            le.minHeight = 0f;

            var tmp = segGo.AddComponent<TextMeshProUGUI>();
            tmp.text = segment.Trim();
            tmp.color = Color.white;
            tmp.fontSize = bodyFontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Overflow;
            ApplyTmpFont(tmp);

            tmp.ForceMeshUpdate();
            var pref = tmp.GetPreferredValues(estimatedTextWidth, 0f);
            le.preferredHeight = Mathf.Max(pref.y + 8f, 40f);

            var cg = segGo.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;
            _segmentGroups.Add(cg);
        }

        if (showSkipHint)
        {
            var hintGo = new GameObject("SkipHint");
            hintGo.transform.SetParent(canvasGo.transform, false);
            var hintRect = hintGo.AddComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0.5f, 0.06f);
            hintRect.anchorMax = new Vector2(0.5f, 0.06f);
            hintRect.pivot = new Vector2(0.5f, 0f);
            hintRect.anchoredPosition = Vector2.zero;
            hintRect.sizeDelta = new Vector2(900f, 48f);

            var hintTmp = hintGo.AddComponent<TextMeshProUGUI>();
            hintTmp.text = skipHintText;
            hintTmp.color = new Color(1f, 1f, 1f, 0.55f);
            hintTmp.fontSize = hintFontSize;
            hintTmp.alignment = TextAlignmentOptions.Bottom;
            ApplyTmpFont(hintTmp);

            var hintCg = hintGo.AddComponent<CanvasGroup>();
            hintCg.alpha = 1f;
        }
    }

    private void ApplyTmpFont(TextMeshProUGUI tmp)
    {
        if (tmp == null)
        {
            return;
        }

        if (fontWithCjkGlyphs != null)
        {
            tmp.font = fontWithCjkGlyphs;
            return;
        }

        if (TMP_Settings.defaultFontAsset != null)
        {
            tmp.font = TMP_Settings.defaultFontAsset;
        }
    }
}
