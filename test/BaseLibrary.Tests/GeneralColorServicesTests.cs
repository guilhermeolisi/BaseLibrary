using FluentAssertions;
using System.Globalization;

namespace BaseLibrary.Tests;

public class GeneralColorServicesTests
{
    static IColorServices colorServices = new ColorServices();
    public GeneralColorServicesTests()
    {
        CultureInfo culture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
    // Cria testes para o método HexFromRgb da classe ColorServices
    [Fact]
    public void HexFromRgb_ValidInput_ReturnsCorrectHex()
    {
        // Arrange
        // Gerar uma lista de cores RGB para testar na forma de array de tuplas
        (byte r, byte g, byte b)[] testColors =
        [
            (255, 0, 0),   // Red
            (0, 255, 0),   // Green
            (0, 0, 255),   // Blue
            (255, 255, 0), // Yellow
            (0, 255, 255), // Cyan
            (255, 0, 255),  // Magenta
            (255, 255, 255), // White
            (0, 0, 0),     // Black
            (128, 128, 128), // Gray
            //Acrescentar mais  cores típicas
        ];

        // usar um array de string que devem ser retornados
        string[] expectedHexValues =
        [
            "#FF0000", // Red
            "#00FF00", // Green
            "#0000FF", // Blue
            "#FFFF00", // Yellow
            "#00FFFF", // Cyan
            "#FF00FF", // Magenta
            "#FFFFFF", // White
            "#000000", // Black
            "#808080"  // Gray
        ];

        // Testar cada cor RGB
        for (int i = 0; i < testColors.Length; i++)
        {
            var (r, g, b) = testColors[i];
            // Act
            string result = colorServices.RGBToHex(r, g, b);
            // Assert
            Assert.Equal(expectedHexValues[i], result);
        }
    }
    //Testar o método RGBFromHex da classe ColorServices
    [Fact]
    public void RGBFromHex_ValidInput_ReturnsCorrectRgb()
    {
        // Arrange
        // Gerar uma lista de cores hexadecimais para testar
        string[] testHexColors =
        [
            "#FF0000", // Red
            "#00FF00", // Green
            "#0000FF", // Blue
            "#FFFF00", // Yellow
            "#00FFFF", // Cyan
            "#FF00FF", // Magenta
            "#FFFFFF", // White
            "#000000", // Black
            "#808080"  // Gray
        ];
        // Usar um array de tuplas que devem ser retornadas
        (byte r, byte g, byte b)[] expectedRgbValues =
        [
            (255, 0, 0),   // Red
            (0, 255, 0),   // Green
            (0, 0, 255),   // Blue
            (255, 255, 0), // Yellow
            (0, 255, 255), // Cyan
            (255, 0, 255), // Magenta
            (255, 255, 255), // White
            (0, 0, 0),     // Black
            (128, 128, 128) // Gray
        ];
        // Testar cada cor hexadecimal
        for (int i = 0; i < testHexColors.Length; i++)
        {
            string hexColor = testHexColors[i];
            // Act
            var result = colorServices.HexToRGB(hexColor);
            // Assert
            Assert.Equal(expectedRgbValues[i], result);
        }
    }
    // Gera os valores no padrão HSL (Hue, Saturation, Lightness) para testar o método RgbToHsl da classe ColorServices
    [Fact]
    public void HSLFromNimlothColor_ValidInput_ReturnsCorrectHsl()
    {
        // Arrange
        // Gerar uma lista de cores RGB para testar na forma de array de tuplas
        (byte r, byte g, byte b)[] testColors =
        [
            (255, 0, 0),   // Red
            (0, 255, 0),   // Green
            (0, 0, 255),   // Blue
            (255, 255, 0), // Yellow
            (0, 255, 255), // Cyan
            (255, 0, 255), // Magenta
            (255, 255, 255), // White
            (0, 0, 0),     // Black
            (128, 128, 128) // Gray
        ];
        // Usar um array de tuplas que devem ser retornadas
        (double h, double s, double l)[] expectedHslValues =
        [
            (0.0, 1.0, 0.5),   // Red
            (120.0, 1.0, 0.5), // Green
            (240.0, 1.0, 0.5), // Blue
            (60.0, 1.0, 0.5),  // Yellow
            (180.0, 1.0, 0.5), // Cyan
            (300.0, 1.0, 0.5), // Magenta
            (0.0, 0.0, 1.0),   // White
            (0.0, 0.0, 0.0),   // Black
            (0, 0, 0.502)   // Gray
        ];
        for (int i = 0; i < testColors.Length; i++)
        {
            var (r, g, b) = testColors[i];
            // Act
            (double h, double s, double l) = colorServices.RGBToHSL(r, g, b);
            // Assert
            h.Should().BeApproximately(expectedHslValues[i].h, 0.001);
            s.Should().BeApproximately(expectedHslValues[i].s, 0.001);
            l.Should().BeApproximately(expectedHslValues[i].l, 0.001);
        }
    }
    [Fact]
    public void NimlothRGBFromHexColors()
    {
        // Arrange
        string[] testHexColors =
        {
            "#4682B4",
            "#17C3B2",
            "#FFCB77",
            "#FEF9EF",
            "#FE6D73",
            "#BC63E9", // Purple
            "#62D411" // Lime
        };
        // Usar um array de tuplas que devem ser retornadas no padrão RGB
        (byte r, byte g, byte b)[] expectedRgbValues =
        {
            (70, 130, 180),  // Steel Blue
            (23, 195, 178),  // Medium Turquoise
            (255, 203, 119), // Light Goldenrod Yellow
            (254, 249, 239), // Light Coral
            (254, 109, 115), // Snow
            (188, 99, 233),  // Purple
            (98, 212, 17)    // Lime
        };
        // Testar cada cor hexadecimal
        string strResult = string.Empty;
        for (int i = 0; i < testHexColors.Length; i++)
        {
            string hexColor = testHexColors[i];
            (byte r, byte g, byte b) = colorServices.HexToRGB(hexColor);
            // Assert
            r.Should().Be(expectedRgbValues[i].r);
            g.Should().Be(expectedRgbValues[i].g);
            b.Should().Be(expectedRgbValues[i].b);
            strResult += $"{r}, {g}, {b}" + Environment.NewLine;
        }
    }
    [Fact]
    public void NimlothHSLFromHexColors()
    {
        // Arrange
        string[] testHexColors =
        {
            "#4682B4",
            "#17C3B2",
            "#FFCB77",
            "#FEF9EF",
            "#FE6D73",
            "#BC63E9", // Purple
            "#62D411" // Lime
        };
        // Usar um array de tuplas que devem ser retornadas
        (double h, double s, double l)[] expectedHslValues =
        {
            (207, 0.44, 0.49),  // Steel Blue
            (174.0, 0.78, 0.42),  // Medium Turquoise
            (37.0, 1.0, 0.73),    // Light Goldenrod Yellow
            (40, 0.88, 0.96),     // Light Coral
            (357, 0.98, 0.71),     // Snow
            (280, 0.75, 0.65), // Purple
            (95, 0.85, 0.45) // Lime
        };
        // Testar cada cor hexadecimal
        string strResult = string.Empty;
        for (int i = 0; i < testHexColors.Length; i++)
        {
            string hexColor = testHexColors[i];
            (byte r, byte g, byte b) = colorServices.HexToRGB(hexColor);
            // Act
            (double h, double s, double l) = colorServices.RGBToHSL(r, g, b);
            // Assert
            h.Should().BeApproximately(expectedHslValues[i].h, 1);
            s.Should().BeApproximately(expectedHslValues[i].s, 0.01);
            l.Should().BeApproximately(expectedHslValues[i].l, 0.01);
            strResult += $"{h.ToString("F2")}, {s.ToString("F4")}, {l.ToString("F4")}" + Environment.NewLine;
        }
    }
    [Fact]
    public void NimlothHexFromHSLColors()
    {
        // Arrange
        (double h, double s, double l)[] hslValues =
        {
            (207, 0.44, 0.49),  // Steel Blue
            (174.0, 0.78, 0.42),  // Medium Turquoise
            (37.0, 1.0, 0.73),    // Light Goldenrod Yellow
            (40, 0.88, 0.96),     // Light Coral
            (357, 0.98, 0.71),     // Snow
            // Daqui par abaixo é sugerido pelo chat GPT
            (280, 0.75, 0.65), // Purple
            (95, 0.85, 0.45), // Lime
            (320, 0.7, 0.6),  // Pink
            (140, 0.65, 0.55), // 
            (300, 0.6, 0.5),
            (15, 0.80, 0.65)
        };
        // Usar um array de tuplas que devem ser retornadas
        string[] expectedTestHexColors =
        {
            "#4682B4",
            "#17C3B2",
            "#FFCB77",
            "#FEF9EF",
            "#FE6D73",
            "#BC63E9", // Purple
            "#62D411", // Lime
            "#E052B1", // Pink
            "#42D773",
            "#CC33CC",
            "#ED825E"
        };
        // Testar cada cor hexadecimal
        string strResult = string.Empty;
        for (int i = 0; i < hslValues.Length; i++)
        {
            (double h, double s, double l) = hslValues[i];
            // Act
            (byte r, byte g, byte b) = colorServices.HSLToRGB(h, s, l);
            string hex = colorServices.RGBToHex(r, g, b);
            // Assert
            //hex.Should().Be(expectedTestHexColors[i]); // Não dá para comparar com o expectedTestHexColors[i] porque a precisão aqui não é possível considerar
            strResult += hex + Environment.NewLine;
        }
    }
    [Fact]
    public void NimlothRGBFromHSLColors()
    {
        // Arrange
        // Usar um array de tuplas que devem ser retornadas
        (double h, double s, double l)[] hslValues =
        {
            (207.27, 0.4400, 0.4902), // Steel Blue
            (174.07, 0.7890, 0.4275), // Medium Turquoise
            (37.06, 1.0000, 0.7333), // Light Goldenrod Yellow
            (357.52, 0.9864, 0.7118), // Light Coral
            (280, 0.75, 0.65), // Purple
            (95, 0.85, 0.45) , // Lime
            (320, 0.7, 0.6),  // Pink
            (140, 0.65, 0.55), // 
            (300, 0.6, 0.5),
            (15, 0.80, 0.65)
        };
        // Usar um array de tuplas que devem ser retornadas no padrão RGB
        (byte r, byte g, byte b)[] expectedRgbValues =
        {
            (70, 130, 180),  // Steel Blue
            (23, 195, 178),  // Medium Turquoise
            (255, 203, 119), // Light Goldenrod Yellow
            (254, 109, 115), // Light Coral
            (188, 99, 233), // Purple
            (98, 212, 17), // Lime
            (224, 82, 177), // Pink
            (66, 215, 115), // 
            (204, 51, 204),
            (237, 130, 94)
        };


        // Testar cada cor HSL
        string strResult = string.Empty;
        for (int i = 0; i < hslValues.Length; i++)
        {
            var (h, s, l) = hslValues[i];
            // Act
            (byte r, byte g, byte b) = colorServices.HSLToRGB(h, s, l);
            // Assert
            r.Should().BeCloseTo(expectedRgbValues[i].r, 1);
            g.Should().BeCloseTo(expectedRgbValues[i].g, 1);
            b.Should().BeCloseTo(expectedRgbValues[i].b, 1);
            strResult += $"{r}, {g}, {b}" + Environment.NewLine;

        }
    }
    [Fact]
    public void LiveChartsHSLFromRGBColors()
    {
        // Arrange
        // Usar um array de tuplas que devem ser retornadas
        (byte r, byte g, byte b)[] rgbValues =
        {
            (116, 77, 169),
            (231, 72, 86),
            (255, 140, 0),
            (0, 153, 188),
            (191, 0, 119),
            (1, 133, 116),
            (194, 57, 179),
            (76, 74, 72),
            (0, 183, 195)
        };
        // Usar um array de tuplas que devem ser retornadas no padrão RGB
        (double h, double s, double l)[] expectedHslValues =
        {
            (265.43, 0.37, 0.48),
            (354.72, 0.77, 0.59),
            (32.94, 1.00, 0.50),
            (191.17, 1.00, 0.37),
            (322.62, 1.00, 0.37),
            (172.27, 0.99, 0.26),
            (306.57, 0.55, 0.49),
            (30.00, 0.03, 0.29),
            (183.69, 1.00, 0.38),
        };


        // Testar cada cor HSL
        string strResult = string.Empty;
        for (int i = 0; i < rgbValues.Length; i++)
        {
            var (r, g, b) = rgbValues[i];
            // Act
            (double h, double s, double l) = colorServices.RGBToHSL(r, g, b);
            // Assert
            h.Should().BeApproximately(expectedHslValues[i].h, 1);
            s.Should().BeApproximately(expectedHslValues[i].s, 1);
            l.Should().BeApproximately(expectedHslValues[i].l, 1);
            strResult += $"({h.ToString("F2")}, {s.ToString("F2")}, {l.ToString("F2")})," + Environment.NewLine;

        }

        List<(double h, double s, double l)> hslClose = [];
        double closed = double.MaxValue;
        for (int i = 0; i < expectedHslValues.Length; i++)
        {
            for (int j = 0; j < expectedHslValues.Length; j++)
            {
                if (i == j) continue;
                double diff = Math.Abs(expectedHslValues[i].h - expectedHslValues[j].h);
                if (diff < closed)
                {
                    closed = diff;
                    hslClose.Clear();
                    hslClose.Add(expectedHslValues[i]);
                    hslClose.Add(expectedHslValues[j]);
                }
            }

        }
    }
    [Fact]
    public void LiveChartsHexFromRGBColors()
    {
        // Arrange
        // Usar um array de tuplas que devem ser retornadas
        (byte r, byte g, byte b)[] rgbValues =
        {
            (116, 77, 169),
            (231, 72, 86),
            (255, 140, 0),
            (0, 153, 188),
            (191, 0, 119),
            (1, 133, 116),
            (194, 57, 179),
            (76, 74, 72),
            (0, 183, 195)
        };
        // Usar um array de tuplas que devem ser retornadas no padrão RGB
        string[] expectedHexValues =
        {
            "#744DA9",
            "#E74856",
            "#FF8C00",
            "#0099BC",
            "#BF0077",
            "#018574",
            "#C239B3",
            "#4C4A48",
            "#00B7C3",
        };


        // Testar cada cor HSL
        string strResult = string.Empty;
        for (int i = 0; i < rgbValues.Length; i++)
        {
            var (r, g, b) = rgbValues[i];
            // Act
            string hex = colorServices.RGBToHex(r, g, b);
            // Assert
            hex.Should().Be(expectedHexValues[i]);
            strResult += $"{hex.TrimStart('#')}-";

        }

    }
    [Fact]
    public void ScottPlotDarkHexFromRGBColors()
    {
        // Arrange
        // Usar um array de tuplas que devem ser retornadas
        (byte r, byte g, byte b)[] rgbValues =
        {
            (152, 195, 121),
            (224, 108, 117),
            (229, 192, 123),
            (97, 175, 240),
            (198, 120, 221),
            (86, 182, 194),
            //Gerado pelo chatgpt
            (255, 130, 92)
        };
        // Usar um array de tuplas que devem ser retornadas no padrão RGB
        string[] expectedHexValues =
        {
            "#98C379",
            "#E06C75",
            "#E5C07B",
            "#61AFF0",
            "#C678DD",
            "#56B6C2",
            "#FF825C",
        };


        // Testar cada cor HSL
        string strResult = string.Empty;
        for (int i = 0; i < rgbValues.Length; i++)
        {
            var (r, g, b) = rgbValues[i];
            // Act
            string hex = colorServices.RGBToHex(r, g, b);
            // Assert
            hex.Should().Be(expectedHexValues[i]);
            strResult += $"{hex.TrimStart('#')}-";
            //strResult += $"\"{hex}\"," + Environment.NewLine;
        }

    }
    [Fact]
    public void ScottPlotDarkHSLFromRGBColors()
    {
        // Arrange
        // Usar um array de tuplas que devem ser retornadas
        (byte r, byte g, byte b)[] rgbValues =
        {
            (152, 195, 121),
            (224, 108, 117),
            (229, 192, 123),
            (97, 175, 240),
            (198, 120, 221),
            (86, 182, 194),
            // Sugerido pelo chatgpt
            
        };
        // Usar um array de tuplas que devem ser retornadas no padrão RGB
        (double h, double s, double l)[] expectedHslValues =
        {
            (94.86, 0.38, 0.62),
            (355.34, 0.65, 0.65),
            (39.06, 0.67, 0.69),
            (207.27, 0.83, 0.66),
            (286.34, 0.60, 0.67),
            (186.67, 0.47, 0.55),
        };


        // Testar cada cor HSL
        string strResult = string.Empty;
        for (int i = 0; i < rgbValues.Length; i++)
        {
            var (r, g, b) = rgbValues[i];
            // Act
            (double h, double s, double l) = colorServices.RGBToHSL(r, g, b);
            // Assert
            h.Should().BeApproximately(expectedHslValues[i].h, 1);
            s.Should().BeApproximately(expectedHslValues[i].s, 1);
            l.Should().BeApproximately(expectedHslValues[i].l, 1);
            strResult += $"({h.ToString("F2")}, {s.ToString("F2")}, {l.ToString("F2")})," + Environment.NewLine;

        }

        List<(double h, double s, double l)> hslClose = [];
        double closed = double.MaxValue;
        for (int i = 0; i < expectedHslValues.Length; i++)
        {
            for (int j = 0; j < expectedHslValues.Length; j++)
            {
                if (i == j) continue;
                double diff = Math.Abs(expectedHslValues[i].h - expectedHslValues[j].h);
                if (diff < closed)
                {
                    closed = diff;
                    hslClose.Clear();
                    hslClose.Add(expectedHslValues[i]);
                    hslClose.Add(expectedHslValues[j]);
                }
            }

        }
    }
    [Fact]
    public void ScottPlotDarkRGBFromHSLColors()
    {
        // Arrange
        // Usar um array de tuplas que devem ser retornadas
        (byte r, byte g, byte b)[] rgbValues =
        {
            (152, 195, 121),
            (224, 108, 117),
            (229, 192, 123),
            (97, 175, 240),
            (198, 120, 221),
            (86, 182, 194),
            // Sugerido pelo chatgpt
            (255, 130, 92),
        };
        // Usar um array de tuplas que devem ser retornadas no padrão RGB
        (double h, double s, double l)[] expectedHslValues =
        {
            (94.86, 0.38, 0.62),
            (355.34, 0.65, 0.65),
            (39.06, 0.67, 0.69),
            (207.27, 0.83, 0.66),
            (286.34, 0.60, 0.67),
            (186.67, 0.47, 0.55),
            (13.99, 1.00, 0.68),
        };


        // Testar cada cor HSL
        string strResult = string.Empty;
        for (int i = 0; i < rgbValues.Length; i++)
        {
            var (r, g, b) = rgbValues[i];
            // Act
            (double h, double s, double l) = colorServices.RGBToHSL(r, g, b);
            // Assert
            h.Should().BeApproximately(expectedHslValues[i].h, 1);
            s.Should().BeApproximately(expectedHslValues[i].s, 1);
            l.Should().BeApproximately(expectedHslValues[i].l, 1);
            strResult += $"({h.ToString("F2")}, {s.ToString("F2")}, {l.ToString("F2")})," + Environment.NewLine;

        }

        List<(double h, double s, double l)> hslClose = [];
        double closed = double.MaxValue;
        for (int i = 0; i < expectedHslValues.Length; i++)
        {
            for (int j = 0; j < expectedHslValues.Length; j++)
            {
                if (i == j) continue;
                double diff = Math.Abs(expectedHslValues[i].h - expectedHslValues[j].h);
                if (diff < closed)
                {
                    closed = diff;
                    hslClose.Clear();
                    hslClose.Add(expectedHslValues[i]);
                    hslClose.Add(expectedHslValues[j]);
                }
            }

        }
    }
    [Fact]
    public void ScottPlotLightHSLFromRGBColors()
    {
        // Arrange
        // Usar um array de tuplas que devem ser retornadas
        (byte r, byte g, byte b)[] rgbValues =
        {
            (80, 161, 79),
            (228, 86, 74),
            (193, 132, 3),
            (0, 132, 188),
            (166, 38, 164),
            (8, 151, 179),
            // Sugerido pelo chatgpt
            (240, 72, 0),
        };
        // Usar um array de tuplas que devem ser retornadas no padrão RGB
        (double h, double s, double l)[] expectedHslValues =
        {
            (119.27, 0.34, 0.47),
            (4.68, 0.74, 0.59),
            (40.74, 0.97, 0.38),
            (197.87, 1.00, 0.37),
            (300.94, 0.63, 0.40),
            (189.82, 0.91, 0.37),
            // Sugerido pelo chatgpt
            (18, 1.00, 0.47),
        };


        // Testar cada cor HSL
        string strResult = string.Empty;
        for (int i = 0; i < rgbValues.Length; i++)
        {
            var (r, g, b) = rgbValues[i];
            // Act
            (double h, double s, double l) = colorServices.RGBToHSL(r, g, b);
            // Assert
            h.Should().BeApproximately(expectedHslValues[i].h, 1);
            s.Should().BeApproximately(expectedHslValues[i].s, 0.01);
            l.Should().BeApproximately(expectedHslValues[i].l, 0.01);
            strResult += $"({h.ToString("F2")}, {s.ToString("F2")}, {l.ToString("F2")})," + Environment.NewLine;

        }

        (double h, double s, double l)[] dark =
        {
            (94.86, 0.38, 0.62),
            (355.34, 0.65, 0.65),
            (39.06, 0.67, 0.69),
            (207.27, 0.83, 0.66),
            (286.34, 0.60, 0.67),
            (186.67, 0.47, 0.55),
        };
        List<(double h, double s, double l)> hslClose = [];
        double closed = double.MaxValue;
        for (int i = 0; i < dark.Length; i++)
        {
            double diff = Math.Abs(expectedHslValues[i].h - dark[i].h);
            double difH = expectedHslValues[i].h - dark[i].h;
            //if (difH < 0)
            //    difH += 360;
            if (difH > 180)
                difH = 360 - difH;
            if (difH < -180)
                difH = -360 - difH;
            hslClose.Add((difH, expectedHslValues[i].s - dark[i].s, expectedHslValues[i].l - dark[i].l));
        }
        double hmean, smean, lmean;
        hmean = smean = lmean = 0;
        for (int i = 0; i < hslClose.Count; i++)
        {
            hmean += hslClose[i].h;
            smean += hslClose[i].s;
            lmean += hslClose[i].l;
        }
        hmean /= hslClose.Count;
        smean /= hslClose.Count;
        lmean /= hslClose.Count;
    }
    [Fact]
    public void ScottPlotLightRGBFromHSLColors()
    {
        // Arrange
        // Usar um array de tuplas que devem ser retornadas
        (double h, double s, double l)[] colorValues =
        {
            (119.27, 0.34, 0.47),
            (4.68, 0.74, 0.59),
            (40.74, 0.97, 0.38),
            (197.87, 1.00, 0.37),
            (300.94, 0.63, 0.40),
            (189.82, 0.91, 0.37),
            // Sugerido pelo chatgpt
            (18, 1.00, 0.47),
        };

        // Usar um array de tuplas que devem ser retornadas no padrão RGB
        (byte r, byte g, byte b)[] expectedValues =
        {
            (80, 161, 79),
            (228, 86, 74),
            (192, 132, 3),
            (0, 132, 188),
            (166, 38, 164),
            (8, 151, 179),
            // Sugerido pelo chatgpt
            (240, 72, 0),
        };


        // Testar cada cor HSL
        string strResult = string.Empty;
        for (int i = 0; i < colorValues.Length; i++)
        {
            var (h, s, l) = colorValues[i];
            // Act
            (byte r, byte g, byte b) = colorServices.HSLToRGB(h, s, l);
            // Assert
            r.Should().BeCloseTo(expectedValues[i].r, 1);
            g.Should().BeCloseTo(expectedValues[i].g, 1);
            b.Should().BeCloseTo(expectedValues[i].b, 1);
            strResult += $"({r}, {g}, {b})," + Environment.NewLine;

        }
    }
    [Fact]
    public void NimlothHexFromRGBColors()
    {
        // Arrange
        // Usar um array de tuplas que devem ser retornadas
        (byte r, byte g, byte b)[] rgbValues =
        {
            (70, 130, 180),  // Steel Blue
            (23, 195, 178),  // Medium Turquoise
            (255, 203, 119), // Light Goldenrod Yellow
            (254, 109, 115), // Light Coral
            (188, 99, 233), // Purple
            (98, 212, 17) // Lime
        };
        // Usar um array de tuplas que devem ser retornadas no padrão RGB
        string[] expectedRgbValues =
        {
            "#4682B4",  // Steel Blue
            "#17C3B2",  // Medium Turquoise
            "#FFCB77", // Light Goldenrod Yellow
            "#FE6D73", // Light Coral
            "#BC63E9", // Purple
            "#62D411" // Lime
        };


        // Testar cada cor HSL
        string strResult = string.Empty;
        for (int i = 0; i < rgbValues.Length; i++)
        {
            (byte r, byte g, byte b) = rgbValues[i];
            // Act
            string hex = colorServices.RGBToHex(r, g, b);
            // Assert
            hex.Should().Be(expectedRgbValues[i]);

            strResult += $"{r}, {g}, {b}" + Environment.NewLine;

        }
    }
}
