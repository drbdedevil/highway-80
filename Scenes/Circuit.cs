using Godot;
using System;
using System.Collections.Generic;

public class SegmentColors
{
    public Color Road { get; set; }
    public Color Grass { get; set; }
    public Color Rumble { get; set; }
    public Color? Lane { get; set; }  // lane может отсутствовать
}

// Точка сегмента (мир и экран)
public class SegmentPoint
{
    public Vector3 World { get; set; }   // x, y, z
    public Vector2 ScreenPos { get; set; } // x, y
    public float ScreenW { get; set; }     // w
    public float Scale { get; set; }
}

// Сам сегмент
public class RoadSegment
{
    public int Index { get; set; }
    public SegmentPoint Point { get; set; }
    public SegmentColors Colors { get; set; }
}

public class Circuit
{
    public MainScene mainScene = null;
    private List<RoadSegment> roadSegments = new List<RoadSegment>();
    private int segmentLength = 100;
    private int roadWidth = 1000;
    private int rumbleSegments = 5;
    private int roadLanes = 3;
    private float cameraHeight = 600f;   // высота камеры над дорогой
    private float cameraDepth = 1.0f;    // глубина перспективы
    private float cameraX = 0f;
    public float cameraZ = 0f;

    private const float DASH_LENGTH = 15f;      // длина штриха в мировых единицах (например, 15)
    private const float GAP_LENGTH = 15f;       // длина промежутка (15)
    private const float DASH_SEGMENT_STEP = 2f;
    
    private SegmentColors lightColors = new SegmentColors
    {
        Road = new Color(0x88, 0x88, 0x88),
        Grass = new Color(0x42, 0x93, 0x52),
        Rumble = new Color(0xb8, 0x31, 0x2e),
        Lane = null
    };
    
    private SegmentColors darkColors = new SegmentColors
    {
        Road = new Color(0x66, 0x66, 0x66),
        Grass = new Color(0x39, 0x7d, 0x46),
        Rumble = new Color(0xDD, 0xDD, 0xDD),
        Lane = new Color(0xFF, 0xFF, 0xFF)
    };
    
    public List<RoadSegment> GetSegments() => roadSegments;
    public int RoadLanes => roadLanes;
    public int RumbleSegments => rumbleSegments;

    public Circuit(MainScene inMainScene)
    {
        mainScene = inMainScene;
    }

    public void create()
    {
        roadSegments = new List<RoadSegment>();
        createRoad();
    }

    private void createRoad()
    {
        int totalSegments = 5000;
        int segmentsPerSection = 100; // размер одной секции
        
        for (int i = 0; i < totalSegments / segmentsPerSection; i++)
        {
            createSection(segmentsPerSection);
        }
        
        // Если totalSegments не кратен segmentsPerSection, добавим остаток
        int remainder = totalSegments % segmentsPerSection;
        if (remainder > 0)
            createSection(remainder);
    }

    private void createSection(int segments)
    {
        int startIndex = roadSegments.Count;
        for (int i = 0; i < segments; i++)
        {
            createSegment(startIndex + i);
        }
    }

    public float GetTotalLength()
    {
        return roadSegments.Count * segmentLength;
    }

    private void createSegment(int index)
    {
        SegmentColors segColors = (index / rumbleSegments) % 2 == 0 ? lightColors : darkColors;

        // Плавный изгиб дороги (синусоида)
        float curve = Mathf.Sin(index * 0.025f) * 500f;

        RoadSegment newSegment = new RoadSegment
        {
            Index = index,
            Point = new SegmentPoint
            {
                World = new Vector3(curve, 0, index * segmentLength),
                ScreenPos = Vector2.Zero,
                ScreenW = 0,
                Scale = -1
            },
            Colors = segColors
        };

        roadSegments.Add(newSegment);
    }

    public void render3D()
    {
        if (roadSegments.Count < 2) return;

        // Сначала проекция всех сегментов
        foreach (var seg in roadSegments)
        {
            Project3D(seg.Point, cameraHeight, cameraDepth, cameraX);
        }

        // Рисуем от дальнего к ближнему (по убыванию World.Z)
        for (int i = roadSegments.Count - 1; i >= 1; i--)
        {
            var curr = roadSegments[i];   // дальний (верх трапеции)
            var prev = roadSegments[i-1]; // ближний (низ трапеции)

            // DrawSegment(
            //     curr.Point.ScreenPos.X, curr.Point.ScreenPos.Y, curr.Point.ScreenW,
            //     prev.Point.ScreenPos.X, prev.Point.ScreenPos.Y, prev.Point.ScreenW,
            //     curr.Colors
            // );

            DrawRoadEdges(curr, prev, Colors.Green);               // сплошные края
            DrawLaneDividers(curr, prev, Colors.Green);
        }
    }

    private void DrawSegment(float x1, float y1, float w1,
                         float x2, float y2, float w2,
                         SegmentColors colors)
    {
        Vector2 leftTop     = new Vector2(x1 - w1, y1);
        Vector2 rightTop    = new Vector2(x1 + w1, y1);
        Vector2 rightBottom = new Vector2(x2 + w2, y2);
        Vector2 leftBottom  = new Vector2(x2 - w2, y2);

        DrawPolygon(leftTop, rightTop, rightBottom, leftBottom, colors.Road);
    }

    private void DrawPolygon(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Color color)
    {
        // Разбиваем четырёхугольник на два треугольника
        // Треугольник 1: p1, p2, p3
        // Треугольник 2: p1, p3, p4
        DrawTriangleIfValid(p1, p2, p3, color);
        DrawTriangleIfValid(p1, p3, p4, color);
    }

    private void DrawTriangleIfValid(Vector2 a, Vector2 b, Vector2 c, Color color)
    {
        // Проверка на совпадение точек
        if (a.DistanceSquaredTo(b) < 0.1f || 
            a.DistanceSquaredTo(c) < 0.1f || 
            b.DistanceSquaredTo(c) < 0.1f)
            return;

        // Увеличил порог с 0.001f до 1.0f (так как координаты экранные)
        float area = Mathf.Abs((b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y));
        if (area < 1.0f) return;

        // Остальные проверки без изменений...
        if (float.IsNaN(a.X) || float.IsNaN(a.Y) ||
            float.IsNaN(b.X) || float.IsNaN(b.Y) ||
            float.IsNaN(c.X) || float.IsNaN(c.Y))
            return;
        if (float.IsInfinity(a.X) || float.IsInfinity(a.Y) ||
            float.IsInfinity(b.X) || float.IsInfinity(b.Y) ||
            float.IsInfinity(c.X) || float.IsInfinity(c.Y))
            return;

        // Упорядочивание вершин по часовой стрелке
        float signedArea = (b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y);
        if (signedArea < 0)
        {
            Vector2 temp = b;
            b = c;
            c = temp;
        }

        Vector2[] triangle = { a, b, c };
        mainScene.DrawColoredPolygon(triangle, color);
    }

    public void Project3D(SegmentPoint point, float cameraHeight, float cameraDepth, float cameraX)
    {
        Vector2 viewportSize = mainScene.GetViewport().GetVisibleRect().Size;
        float trackLength = GetTotalLength();
        float z = GetZDistance(point.World.Z, cameraZ, trackLength);
        if (z < 0.5f) z = 0.5f;  // защита от деления на ноль
        
        float scale = cameraDepth / z;
        float screenX = viewportSize.X / 2 + (point.World.X - cameraX) * scale;
        float screenY = viewportSize.Y / 2 - (point.World.Y - cameraHeight) * scale;
        point.ScreenPos = new Vector2(screenX, screenY);
        point.ScreenW = roadWidth * scale;
        point.Scale = scale;
    }

    private void DrawDashedCenterLine(RoadSegment curr, RoadSegment prev, Color color)
    {
        DrawRoadEdges(curr, prev, color);
        DrawLaneDividers(curr, prev, color);
        // Если нужна центральная линия дополнительно:
        // DrawCenterLine(curr, prev, Colors.Yellow);
    }

    private void DrawRoadEdges(RoadSegment curr, RoadSegment prev, Color color)
    {
        float topW = curr.Point.ScreenW;
        float bottomW = prev.Point.ScreenW;
        if (topW < 2f || bottomW < 2f) return;

        float edgeWidthTop = topW * 0.08f;    // ширина краевой линии (8% от ширины дороги)
        float edgeWidthBottom = bottomW * 0.08f;

        if (edgeWidthTop < 1f && edgeWidthBottom < 1f) return;

        Vector2 centerTop = curr.Point.ScreenPos;
        Vector2 centerBottom = prev.Point.ScreenPos;

        // Левая краевая линия
        Vector2 leftTopOuter = new Vector2(centerTop.X - topW, centerTop.Y);
        Vector2 leftTopInner = new Vector2(centerTop.X - topW + edgeWidthTop, centerTop.Y);
        Vector2 leftBottomInner = new Vector2(centerBottom.X - bottomW + edgeWidthBottom, centerBottom.Y);
        Vector2 leftBottomOuter = new Vector2(centerBottom.X - bottomW, centerBottom.Y);
        DrawPolygon(leftTopOuter, leftTopInner, leftBottomInner, leftBottomOuter, color);

        // Правая краевая линия
        Vector2 rightTopInner = new Vector2(centerTop.X + topW - edgeWidthTop, centerTop.Y);
        Vector2 rightTopOuter = new Vector2(centerTop.X + topW, centerTop.Y);
        Vector2 rightBottomOuter = new Vector2(centerBottom.X + bottomW, centerBottom.Y);
        Vector2 rightBottomInner = new Vector2(centerBottom.X + bottomW - edgeWidthBottom, centerBottom.Y);
        DrawPolygon(rightTopInner, rightTopOuter, rightBottomOuter, rightBottomInner, color);
    }

    private void DrawLaneDividers(RoadSegment curr, RoadSegment prev, Color color)
    {
        float topW = curr.Point.ScreenW;
        float bottomW = prev.Point.ScreenW;
        if (topW < 3f || bottomW < 3f) return;

        int lanes = roadLanes;
        if (lanes <= 1) return;

        // Получаем мировые Z текущего и предыдущего сегмента
        float zCurr = curr.Point.World.Z;
        float zPrev = prev.Point.World.Z;
        float zMin = Math.Min(zCurr, zPrev);
        float zMax = Math.Max(zCurr, zPrev);
        float segmentLengthWorld = zMax - zMin; // обычно = segmentLength (100 или 30)
        if (segmentLengthWorld <= 0) return;

        // Разбиваем сегмент на маленькие кусочки для отрисовки штрихов и пропусков
        float step = DASH_SEGMENT_STEP; // шаг разбиения (чем меньше, тем плавнее края)
        for (float z = zMin; z < zMax; z += step)
        {
            // Определяем, где мы находимся внутри цикла штрих-пропуск
            float cyclePos = (z - zMin) % (DASH_LENGTH + GAP_LENGTH);
            bool isDash = cyclePos < DASH_LENGTH;
            if (!isDash) continue; // рисуем только штрихи

            float zNext = Math.Min(z + step, zMax);
            if (zNext - z < 0.1f) continue;

            // Интерполируем экранные координаты для этих двух Z
            float t1 = (z - zMin) / segmentLengthWorld;
            float t2 = (zNext - zMin) / segmentLengthWorld;
            // точки на текущем сегменте (curr - дальний, prev - ближний)
            // t=0 -> prev (ближний), t=1 -> curr (дальний)
            // Внимание: в render3D мы идём от дальнего к ближнему, но здесь curr - дальний, prev - ближний
            Vector2 p1 = LerpScreen(prev.Point.ScreenPos, curr.Point.ScreenPos, t1);
            Vector2 p2 = LerpScreen(prev.Point.ScreenPos, curr.Point.ScreenPos, t2);
            float w1 = Mathf.Lerp(prev.Point.ScreenW, curr.Point.ScreenW, t1);
            float w2 = Mathf.Lerp(prev.Point.ScreenW, curr.Point.ScreenW, t2);

            // Рисуем все разделители полос для этого маленького отрезка
            float laneStepTop = (w1 * 2) / lanes;
            float laneStepBottom = (w2 * 2) / lanes;
            Vector2 center1 = p1;
            Vector2 center2 = p2;

            for (int i = 1; i < lanes; i++)
            {
                float offset1 = -w1 + i * laneStepTop;
                float offset2 = -w2 + i * laneStepBottom;
                float lineWidth1 = w1 * 0.04f;
                float lineWidth2 = w2 * 0.04f;

                if (lineWidth1 < 0.5f && lineWidth2 < 0.5f) continue;

                Vector2 leftTop = new Vector2(center1.X + offset1 - lineWidth1/2, center1.Y);
                Vector2 rightTop = new Vector2(center1.X + offset1 + lineWidth1/2, center1.Y);
                Vector2 rightBottom = new Vector2(center2.X + offset2 + lineWidth2/2, center2.Y);
                Vector2 leftBottom = new Vector2(center2.X + offset2 - lineWidth2/2, center2.Y);

                DrawPolygon(leftTop, rightTop, rightBottom, leftBottom, color);
            }
        }
    }

    // Вспомогательная линейная интерполяция экранных точек
    private Vector2 LerpScreen(Vector2 a, Vector2 b, float t)
    {
        return new Vector2(
            Mathf.Lerp(a.X, b.X, t),
            Mathf.Lerp(a.Y, b.Y, t)
        );
    }

    private float GetZDistance(float worldZ, float cameraZ, float trackLength)
    {
        float dz = worldZ - cameraZ;
        dz = dz % trackLength;
        if (dz < 0) dz += trackLength;
        if (dz > trackLength * 0.5f) dz -= trackLength;
        return dz;
    }
}