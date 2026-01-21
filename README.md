# ChnDPEncoder 动态规划赛码器

一个轻量级控制台应用程序，用于计算特定中文输入法方案下
整篇文本的最小开销编码，并简要分析该编码的键盘使用情况，
以供输入法设计者和学习者参考。通过动态规划算法和前缀树，
实现在单线程下每秒处理数百万字的极高性能。

[![仓库 GitHub](https://img.shields.io/badge/GitHub-0FBF3E.svg?logo=github)](https://github.com/GarthTB/ChnDPEncoder)
[![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![C# 14.0](https://img.shields.io/badge/C%23-14.0-512BD4.svg)](https://github.com/dotnet/csharplang)
[![目标 win-x64](https://img.shields.io/badge/目标-win--x64-0078D1.svg)](https://learn.microsoft.com/dotnet/core/rid-catalog)
[![协议 MIT](https://img.shields.io/badge/协议-MIT-111)](https://mit-license.org)
[![版本 1.0.0](https://img.shields.io/badge/版本-1.0.0-0C0)](https://github.com/GarthTB/ChnDPEncoder/releases/latest)

## 功能特点

- 📦 AOT 编译：单个 exe 解压即用，无运行时依赖
- 🚀 极致性能：每秒处理数百万字
- 🎯 精确优化：全局 DP 算法确保编码开销最小化
- 📊 编码分析：直接获取30+项编码统计数据

## 使用方法

### 系统要求

- 操作系统：Windows 10+
- 架构：x64

### 使用步骤

1. 下载 [最新发布包](https://github.com/GarthTB/ChnDPEncoder/releases/latest) 并解压
2. 修改配置并保存为程序目录下的 `Config.toml`，作为输入参数
3. 在控制台中运行程序 `ChnDPEncoder.exe` 以监视运行情况，
   或直接双击运行，执行完毕后自动退出
4. 编码及统计数据自动输出到原文本目录下，无覆盖风险

## 配置文件

程序目录下的 `Config.toml` 提供所有输入参数，有且仅有4个字段：

- cost_map：键对开销表，每行格式为 `字符1 字符2 [TAB] 浮点数`
- rime_dict：包含一个输入法方案中所有字词的 [RIME格式](https://github.com/rime/home/wiki/RimeWithSchemata) 词库
- texts：待编码文本路径
- layout：各手指和各排的键值，共14项，有序，详见示例

随包附带示例配置 `DemoConfig.toml`。另附开销表 `CostMap.tsv`，
值为连续击键的时间相对值<sup>[1]</sup>。若全部设为相同值，
则计算结果为码长最短的编码。

## 开源信息

- 作者：Garth TB | 天卜 <g-art-h@outlook.com>
- 许可证：[MIT 许可证](https://mit-license.org/)
- 项目地址：https://github.com/GarthTB/ChnDPEncoder
- 依赖：[Tomlyn](https://github.com/xoofx/Tomlyn)

## 参考文献

[1]陈一凡,张鹿,周志农. 键位相关速度当量的研究[J].中文信息学报,1990,(04):12-18+11.

## 版本日志

### 1.0.0 (20260122) 初始发布
