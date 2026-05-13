using UnityEngine;
using UnityEngine.Scripting;

namespace XCharts.Runtime
{
    [Preserve]
    internal sealed class EffectScatterHandler : BaseScatterHandler<EffectScatter>
    {
        private readonly float m_EffectScatterSpeed = 15;

        public override void Update()
        {
            base.Update();
            var symbolSize = serie.symbol.GetSize(null, chart.theme.serie.scatterSymbolSize);
            var deltaTime = serie.animation.unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            for (var i = 0; i < serie.symbol.animationSize.Count; ++i)
            {
                serie.symbol.animationSize[i] += m_EffectScatterSpeed * deltaTime;
                if (serie.symbol.animationSize[i] > symbolSize) serie.symbol.animationSize[i] = i * 5;
                chart.RefreshPainter(serie);
            }
        }
    }
}