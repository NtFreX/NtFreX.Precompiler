# NtFreX.Precompiler [![NuGet version (NtFreX.Precompiler)](https://img.shields.io/nuget/v/NtFreX.Precompiler.svg)](https://www.nuget.org/packages/NtFreX.Precompiler/)

A generic precompiler for .NET

## Supported syntax:
 - #if
 - #elseif
 - #else
 - #endif
 - not
 - !
 - #include
 - #{VARIABLE}

## Examples:

```
#include ./math.shader
```

```
#if hasInstances
  vec3 color = mix(instanceColor, baseColor, alpha);
#elseif hasTint
  vec3 color = mix(tint, baseColor, alpha);
#else
  vec3 color = baseColor;
#endif
```

```
vec3 color = #{color}
```

```
#if hasInstances #include ./instance.shader #endif
```
