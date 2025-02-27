using System;
using System.IO;
using JetBrains.Annotations;
using OpenTemple.Core.IO;

namespace OpenTemple.Core.MaterialDefinitions;

public class MdfParser
{
    private readonly string _filename;
    private readonly string _content;
    private readonly StringReader _in;

    public bool Strict { get; set; }

    public MdfParser(string filename, string content)
    {
        _filename = filename;
        _content = content;
        _in = new StringReader(content);
    }

    public MdfMaterial Parse()
    {
        var type = ParseMaterialType();

        return type switch
        {
            MdfType.Textured => ParseTextured(),
            MdfType.General => ParseGeneral(),
            MdfType.Clipper => ParseClipper(),
            _ => throw CreateError($"Unrecognized MDF material type: {type}")
        };
    }

    private MdfType ParseMaterialType()
    {
        var line = _in.ReadLine();
        if (line == null)
        {
            throw CreateError("File is empty");
        }
        
        var input = line.Trim().ToLowerInvariant();

        return input switch
        {
            "textured" => MdfType.Textured,
            "general" => MdfType.General,
            "clipper" => MdfType.Clipper,
            _ => throw CreateError("Unrecognized material type '{0}'", input)
        };
    }

    private MdfMaterial ParseClipper()
    {
        var result = new MdfMaterial(MdfType.Clipper);

        result.EnableZWrite = false;
        result.EnableColorWrite = false;

        var tokenizer = new Tokenizer(_content);
        tokenizer.NextToken(); // Skip material type

        while (tokenizer.NextToken())
        {
            if (tokenizer.IsNamedIdentifier("wire"))
            {
                result.Wireframe = true;
                result.EnableColorWrite = true;
            }
            else if (tokenizer.IsNamedIdentifier("zfill"))
            {
                result.EnableZWrite = true;
            }
            else if (tokenizer.IsNamedIdentifier("outline"))
            {
                result.Outline = true;
                result.EnableColorWrite = true;
            }
            else
            {
                if (Strict)
                {
                    throw CreateError("Unrecognized token '{0}'", tokenizer.TokenText.ToString());
                }
            }
        }

        return result;
    }

    private MdfMaterial ParseTextured()
    {
        var result = new MdfMaterial(MdfType.Textured);

        var tokenizer = new Tokenizer(_content);
        /*
            For some reason ToEE doesn't use the tokenizer for this
            shader type normally. So we disable escape sequences to
            get some form of compatibility.
        */
        tokenizer.IsEnableEscapes = false;
        tokenizer.NextToken(); // Skip material type

        while (tokenizer.NextToken())
        {
            if (tokenizer.IsNamedIdentifier("color"))
            {
                if (!ParseRgba(ref tokenizer, "Color", out result.Diffuse))
                {
                    throw CreateError("Unable to parse diffuse color");
                }
            }
            else if (tokenizer.IsNamedIdentifier("texture"))
            {
                if (!tokenizer.NextToken() || !tokenizer.IsQuotedString)
                {
                    throw CreateError("Missing filename for texture");
                }

                result.Samplers[0].Filename = tokenizer.TokenText.ToString();
            }
            else if (tokenizer.IsNamedIdentifier("colorfillonly"))
            {
                result.EnableZWrite = false;
                result.EnableColorWrite = true;
            }
            else if (tokenizer.IsNamedIdentifier("notlit"))
            {
                result.NotLit = true;
            }
            else if (tokenizer.IsNamedIdentifier("notlite"))
            {
                // The original ToEE parser only does prefix parsing, which is why
                // "notlite" was accepted as "notlit".
                result.NotLit = true;
            }
            else if (tokenizer.IsNamedIdentifier("disablez"))
            {
                result.DisableZ = true;
            }
            else if (tokenizer.IsNamedIdentifier("double"))
            {
                result.FaceCulling = false;
            }
            else if (tokenizer.IsNamedIdentifier("clamp"))
            {
                result.Clamp = true;
            }
            else
            {
                if (Strict)
                {
                    throw CreateError("Unrecognized token '{0}'", tokenizer.TokenText.ToString());
                }
            }
        }

        return result;
    }

    private MdfMaterial ParseGeneral()
    {
        var result = new MdfMaterial(MdfType.General);

        var tokenizer = new Tokenizer(_content);
        tokenizer.NextToken(); // Skip material type

        while (tokenizer.NextToken())
        {
            if (!tokenizer.IsIdentifier)
            {
                if (Strict)
                {
                    throw CreateError("Unexpected token: {0}", tokenizer.TokenText.ToString());
                }

                continue;
            }

            if (tokenizer.IsNamedIdentifier("highquality"))
            {
                // In no case is the GPU the bottleneck anymore,
                // so we will always parse the high quality section
                // Previously, it did cancel here if the GPU supported
                // less than 4 textures
                continue;
            }

            if (tokenizer.IsNamedIdentifier("texture"))
            {
                if (!ParseTextureStageId(ref tokenizer))
                {
                    continue;
                }

                var samplerNo = tokenizer.TokenInt;
                if (ParseFilename(ref tokenizer, "Texture"))
                {
                    result.Samplers[samplerNo].Filename = tokenizer.TokenText.ToString();
                }

                continue;
            }

            if (tokenizer.IsNamedIdentifier("glossmap"))
            {
                if (ParseFilename(ref tokenizer, "GlossMap"))
                {
                    result.Glossmap = tokenizer.TokenText.ToString();
                }

                continue;
            }

            if (tokenizer.IsNamedIdentifier("uvtype"))
            {
                if (!ParseTextureStageId(ref tokenizer))
                {
                    continue;
                }

                var samplerNo = tokenizer.TokenInt;

                if (!ParseIdentifier(ref tokenizer, "UvType"))
                {
                    continue;
                }

                MdfUvType uvType;
                if (tokenizer.IsNamedIdentifier("mesh"))
                {
                    uvType = MdfUvType.Mesh;
                }
                else if (tokenizer.IsNamedIdentifier("environment"))
                {
                    uvType = MdfUvType.Environment;
                }
                else if (tokenizer.IsNamedIdentifier("drift"))
                {
                    uvType = MdfUvType.Drift;
                }
                else if (tokenizer.IsNamedIdentifier("swirl"))
                {
                    uvType = MdfUvType.Swirl;
                }
                else if (tokenizer.IsNamedIdentifier("wavey"))
                {
                    uvType = MdfUvType.Wavey;
                }
                else
                {
                    if (Strict)
                    {
                        throw CreateError("Unrecognized UvType: {0}", tokenizer.TokenText.ToString());
                    }

                    continue;
                }

                result.Samplers[samplerNo].UvType = uvType;
                continue;
            }

            if (tokenizer.IsNamedIdentifier("blendtype"))
            {
                if (!ParseTextureStageId(ref tokenizer))
                {
                    continue;
                }

                var samplerNo = tokenizer.TokenInt;

                if (!ParseIdentifier(ref tokenizer, "BlendType"))
                {
                    continue;
                }

                MdfTextureBlendType blendType;
                if (tokenizer.IsNamedIdentifier("modulate"))
                {
                    blendType = MdfTextureBlendType.Modulate;
                }
                else if (tokenizer.IsNamedIdentifier("add"))
                {
                    blendType = MdfTextureBlendType.Add;
                }
                else if (tokenizer.IsNamedIdentifier("texturealpha"))
                {
                    blendType = MdfTextureBlendType.TextureAlpha;
                }
                else if (tokenizer.IsNamedIdentifier("currentalpha"))
                {
                    blendType = MdfTextureBlendType.CurrentAlpha;
                }
                else if (tokenizer.IsNamedIdentifier("currentalphaadd"))
                {
                    blendType = MdfTextureBlendType.CurrentAlphaAdd;
                }
                else
                {
                    if (Strict)
                    {
                        throw CreateError("Unrecognized BlendType: {0}", tokenizer.TokenText.ToString());
                    }

                    continue;
                }

                result.Samplers[samplerNo].BlendType = blendType;
                continue;
            }

            if (tokenizer.IsNamedIdentifier("color"))
            {
                uint argbColor;
                if (ParseRgba(ref tokenizer, "Color", out argbColor))
                {
                    result.Diffuse = argbColor;
                }

                continue;
            }

            if (tokenizer.IsNamedIdentifier("specular"))
            {
                uint argbColor;
                if (ParseRgba(ref tokenizer, "Specular", out argbColor))
                {
                    result.Specular = argbColor;
                }

                continue;
            }

            if (tokenizer.IsNamedIdentifier("specularpower"))
            {
                if (!tokenizer.NextToken())
                {
                    if (Strict)
                    {
                        throw CreateError("Unexpected end of file after SpecularPower");
                    }
                }
                else if (!tokenizer.IsNumber)
                {
                    if (Strict)
                    {
                        throw CreateError("Expected number after SpecularPower, but got: {0}",
                            tokenizer.TokenText.ToString());
                    }
                }
                else
                {
                    result.SpecularPower = tokenizer.TokenFloat;
                }

                continue;
            }

            if (tokenizer.IsNamedIdentifier("materialblendtype"))
            {
                if (!ParseIdentifier(ref tokenizer, "MaterialBlendType"))
                {
                    continue;
                }

                if (tokenizer.IsNamedIdentifier("none"))
                {
                    result.BlendType = MdfBlendType.None;
                }
                else if (tokenizer.IsNamedIdentifier("alpha"))
                {
                    result.BlendType = MdfBlendType.Alpha;
                }
                else if (tokenizer.IsNamedIdentifier("add"))
                {
                    result.BlendType = MdfBlendType.Add;
                }
                else if (tokenizer.IsNamedIdentifier("alphaadd"))
                {
                    result.BlendType = MdfBlendType.AlphaAdd;
                }
                else
                {
                    if (Strict)
                    {
                        throw CreateError("Unrecognized MaterialBlendType: {0}",
                            tokenizer.TokenText.ToString());
                    }
                }

                continue;
            }

            if (tokenizer.IsNamedIdentifier("speed"))
            {
                if (!ParseNumber(ref tokenizer, "Speed"))
                {
                    continue;
                }

                var speed = tokenizer.TokenFloat * 60.0f;

                // Set the speed for all texture stages and for both U and V
                foreach (var sampler in result.Samplers)
                {
                    sampler.SpeedU = speed;
                    sampler.SpeedV = speed;
                }

                continue;
            }

            if (tokenizer.IsNamedIdentifier("speedu"))
            {
                if (!ParseTextureStageId(ref tokenizer))
                {
                    continue;
                }

                var samplerNo = tokenizer.TokenInt;

                if (!ParseNumber(ref tokenizer, "SpeedU"))
                {
                    continue;
                }

                var speed = tokenizer.TokenFloat * 60.0f;
                result.Samplers[samplerNo].SpeedU = speed;
                continue;
            }

            if (tokenizer.IsNamedIdentifier("speedv"))
            {
                if (!ParseTextureStageId(ref tokenizer))
                {
                    continue;
                }

                var samplerNo = tokenizer.TokenInt;

                if (!ParseNumber(ref tokenizer, "SpeedV"))
                {
                    continue;
                }

                var speed = tokenizer.TokenFloat * 60.0f;
                result.Samplers[samplerNo].SpeedV = speed;
                continue;
            }

            if (tokenizer.IsNamedIdentifier("double"))
            {
                result.FaceCulling = false;
                continue;
            }

            if (tokenizer.IsNamedIdentifier("linearfiltering"))
            {
                result.LinearFiltering = true;
                continue;
            }

            if (tokenizer.IsNamedIdentifier("recalculatenormals"))
            {
                result.RecalculateNormals = true;
                continue;
            }

            if (tokenizer.IsNamedIdentifier("zfillonly"))
            {
                result.EnableColorWrite = false;
                result.EnableZWrite = true;
                continue;
            }

            if (tokenizer.IsNamedIdentifier("colorfillonly"))
            {
                result.EnableColorWrite = true;
                result.EnableZWrite = false;
                continue;
            }

            if (tokenizer.IsNamedIdentifier("notlit"))
            {
                result.NotLit = true;
                continue;
            }

            if (tokenizer.IsNamedIdentifier("disablez"))
            {
                result.DisableZ = true;
                continue;
            }

            if (Strict)
            {
                throw CreateError("Unrecognized token: {0}", tokenizer.TokenText.ToString());
            }
        }

        return result;
    }

    private bool ParseTextureStageId(ref Tokenizer tokenizer)
    {
        if (!tokenizer.NextToken())
        {
            if (Strict)
            {
                throw CreateError("Missing argument for texture");
            }

            return false;
        }

        if (!tokenizer.IsNumber || tokenizer.TokenInt < 0 || tokenizer.TokenInt >= 4)
        {
            if (Strict)
            {
                throw CreateError("Expected a texture stage between 0 and 3 as the second argument: {0}",
                    tokenizer.TokenText.ToString());
            }

            return false;
        }

        return true;
    }

    private bool ParseFilename(ref Tokenizer tokenizer, string logMsg)
    {
        if (!tokenizer.NextToken())
        {
            if (Strict)
            {
                throw CreateError("Filename for {0} is missing.", logMsg);
            }

            return false;
        }
        else if (!tokenizer.IsQuotedString)
        {
            if (Strict)
            {
                throw CreateError("Unexpected token instead of filename found for {0}: {1}",
                    logMsg, tokenizer.TokenText.ToString());
            }

            return false;
        }
        else
        {
            return true;
        }
    }

    private bool ParseIdentifier(ref Tokenizer tokenizer, string logMsg)
    {
        if (!tokenizer.NextToken())
        {
            if (Strict)
            {
                throw CreateError("Identifier after {0} expected.", logMsg);
            }

            return false;
        }

        if (!tokenizer.IsIdentifier)
        {
            if (Strict)
            {
                throw CreateError("Identifier after {0} expected, but got: {1}",
                    logMsg, tokenizer.TokenText.ToString());
            }

            return false;
        }

        return true;
    }

    private bool ParseRgba(ref Tokenizer tokenizer, string logMsg, out uint argbOut)
    {
        // Color in the input is RGBA
        argbOut = 0; // The output is ARGB

        if (!tokenizer.NextToken() || !tokenizer.IsNumber)
        {
            if (!Strict)
            {
                return false;
            }

            throw CreateError("Missing red component for {0}", logMsg);
        }

        argbOut |= (uint) ((tokenizer.TokenInt & 0xFF) << 16);

        if (!tokenizer.NextToken() || !tokenizer.IsNumber)
        {
            if (!Strict)
            {
                return false;
            }

            throw CreateError("Missing green component for {0}", logMsg);
        }

        argbOut |= (uint) (tokenizer.TokenInt & 0xFF) << 8;

        if (!tokenizer.NextToken() || !tokenizer.IsNumber)
        {
            if (!Strict)
            {
                return false;
            }

            throw CreateError("Missing blue component for {0}", logMsg);
        }

        argbOut |= (uint) (tokenizer.TokenInt & 0xFF);

        if (!tokenizer.NextToken() || !tokenizer.IsNumber)
        {
            if (!Strict)
            {
                return false;
            }

            throw CreateError("Missing alpha component for {0}", logMsg);
        }

        argbOut |= (uint) (tokenizer.TokenInt & 0xFF) << 24;

        return true;
    }

    private bool ParseNumber(ref Tokenizer tokenizer, string logMsg)
    {
        if (!tokenizer.NextToken())
        {
            if (Strict)
            {
                throw CreateError("Unexpected end of file after {0}", logMsg);
            }

            return false;
        }
        else if (!tokenizer.IsNumber)
        {
            if (Strict)
            {
                throw CreateError("Expected number after {0}, but got: {1}",
                    logMsg, tokenizer.TokenText.ToString());
            }

            return false;
        }

        return true;
    }

    // Creates a better error message with context
    [StringFormatMethod("format")]
    private MdfException CreateError(string format, params object[] args)
    {
        return new MdfException(string.Format(format, args));
    }
}

internal class MdfException : Exception
{
    public MdfException(string message) : base(message)
    {
    }
}