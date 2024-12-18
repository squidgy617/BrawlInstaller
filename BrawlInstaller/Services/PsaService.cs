using BrawlInstaller.Classes;
using BrawlInstaller.StaticClasses;
using BrawlLib.Internal;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Services
{
    public interface IPsaService
    {
        /// <inheritdoc cref="PsaService.UpdateGFXIds(ResourceNode, int, int)"/>
        ResourceNode UpdateGFXIds(ResourceNode movesetDataNode, int effectPacId, int oldEffectPacId);

        /// <inheritdoc cref="PsaService.UpdateSFXIds(ResourceNode, int, int)"/>
        ResourceNode UpdateSFXIds(ResourceNode movesetDataNode, int soundbankId, int oldSoundbankId);
    }

    [Export(typeof(IPsaService))]
    internal class PsaService : IPsaService
    {
        private List<OpCode> OpCodes = new List<OpCode>
        {
            // Graphic Effects
            new OpCode(new byte[4]{0x11, 0x00, 0x10, 0x00}, 0),
            new OpCode(new byte[4]{0x11, 0x01, 0x0A, 0x00 }, 0),
            new OpCode(new byte[4]{0x11, 0x02, 0x0A, 0x00 }, 0),
            new OpCode(new byte[4]{0x11, 0x15, 0x03, 0x00 }, 0),
            new OpCode(new byte[4]{0x11, 0x1A, 0x10, 0x00 }, 0),
            new OpCode(new byte[4]{0x11, 0x1B, 0x10, 0x00 }, 0),
            new OpCode(new byte[4]{0x11, 0x1C, 0x10, 0x00 }, 0),
            new OpCode(new byte[4]{0x0E, 0x0B, 0x02, 0x00 }, 0),

            // Sword Glows
            new OpCode(new byte[4]{0x11, 0x04, 0x17, 0x00 }, 11),
            new OpCode(new byte[4]{0x11, 0x03, 0x14, 0x00 }, 11)
        };

        private List<OpCode> TraceOpCodes = new List<OpCode>()
        {
            new OpCode(new byte[4]{0x11, 0x03, 0x14, 0x00}, 0),
            new OpCode(new byte[4]{0x11, 0x04, 0x17, 0x00}, 0)
        };

        private List<OpCode> SfxOpCodes = new List<OpCode>
        {
            new OpCode(new byte[4]{0x0A, 0x00, 0x01, 0x00}, 0),
            new OpCode(new byte[4]{0x0A, 0x01, 0x01, 0x00}, 0),
            new OpCode(new byte[4]{0x0A, 0x02, 0x01, 0x00}, 0),
            new OpCode(new byte[4]{0x0A, 0x03, 0x01, 0x00}, 0),
            new OpCode(new byte[4]{0x0A, 0x09, 0x01, 0x00}, 0),
            new OpCode(new byte[4]{0x0A, 0x05, 0x01, 0x00}, 0),
            new OpCode(new byte[4]{0x0A, 0x0A, 0x01, 0x00}, 0),
        };

        // Services
        ISettingsService _settingsService { get; }
        IFileService _fileService { get; }

        [ImportingConstructor]
        public PsaService(ISettingsService settingsService, IFileService fileService)
        {
            _settingsService = settingsService;
            _fileService = fileService;
        }

        // Methods

        /// <summary>
        /// Update GFX Effect.pac IDs in moveset data
        /// </summary>
        /// <param name="movesetDataNode">Moveset data to modify</param>
        /// <param name="effectPacId">Effect.pac ID to update to</param>
        /// <param name="oldEffectPacId">Effect.pac ID to replace</param>
        /// <returns></returns>
        public ResourceNode UpdateGFXIds(ResourceNode movesetDataNode, int effectPacId, int oldEffectPacId)
        {
            var data = _fileService.ReadRawData(movesetDataNode);
            data = UpdateGFXIds(data, effectPacId, oldEffectPacId);
            // Only update custom traces
            if (effectPacId >= 311 && oldEffectPacId >= 311)
            {
                data = UpdateTraceIds(data, effectPacId, oldEffectPacId);
            }
            _fileService.ReplaceNodeRaw(movesetDataNode, data);
            return movesetDataNode;
        }

        /// <summary>
        /// Update SFX in moveset data
        /// </summary>
        /// <param name="movesetDataNode">Moveset data to modify</param>
        /// <param name="soundbankId">Soundbank ID to update to</param>
        /// <param name="oldSoundbankId">Soundbank ID to replace</param>
        /// <returns></returns>
        public ResourceNode UpdateSFXIds(ResourceNode movesetDataNode, int soundbankId, int oldSoundbankId)
        {
            var data = _fileService.ReadRawData(movesetDataNode);
            data = UpdateSFXIds(data, soundbankId, oldSoundbankId);
            _fileService.ReplaceNodeRaw(movesetDataNode, data);
            return movesetDataNode;
        }

        //TODO: Should custom GFX/SFX IDs be driven by settings?

        /// <summary>
        /// Update GFX Effect.pac IDs in raw moveset data
        /// </summary>
        /// <param name="data">Raw moveset data</param>
        /// <param name="effectPacId">Effect.pac ID to update to</param>
        /// <param name="oldEffectPacId">Effect.pac ID to replace</param>
        /// <returns></returns>
        private byte[] UpdateGFXIds(byte[] data, int effectPacId, int oldEffectPacId)
        {
            // Loop through each OpCode
            foreach (var opCode in OpCodes)
            {
                var slice = data.AsSpan(0, data.Length);
                // Search data for opcode
                while (true)
                {
                    // Get index of first matching opcode
                    var index = slice.IndexOf(opCode.Bytes);
                    // If invalid index, break
                    if (index <= -1 || index > slice.Length)
                    {
                        break;
                    }
                    // Jump to first matching opcode
                    slice = slice.Slice(slice.IndexOf(opCode.Bytes));
                    // If end of data, break
                    if (slice.Length < 4)
                    {
                        break;
                    }
                    // Skip over opcode to data pointer
                    slice = slice.Slice(4);
                    // Get address from pointer
                    var parameterAddress = Convert.ToUInt32(BitConverter.ToString(slice.ToArray(), 0, 4).Replace("-", ""), 16) + 0x24 + opCode.Offset * 8;
                    // If address is out of reach, continue
                    if (parameterAddress > data.Length)
                    {
                        continue;
                    }
                    // Otherwise, get Effect PAC ID from address
                    var array = data.AsSpan((int)parameterAddress, 2).ToArray().Reverse();
                    var foundEffectPacId = BitConverter.ToUInt16(array.ToArray(), 0);
                    if (foundEffectPacId == oldEffectPacId)
                    {
                        byte[] newBytes = BitConverter.GetBytes((ushort)effectPacId).Reverse().ToArray();
                        newBytes.CopyTo(data, parameterAddress);
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// Update trace IDs in raw moveset data
        /// </summary>
        /// <param name="data">Raw moveset data</param>
        /// <param name="effectPacId">Effect.pac ID to update to</param>
        /// <param name="oldEffectPacId">Effect.pac ID to replace</param>
        /// <returns></returns>
        private byte[] UpdateTraceIds(byte[] data, int effectPacId, int oldEffectPacId)
        {
            // Loop through each OpCode
            foreach (var opCode in TraceOpCodes)
            {
                var slice = data.AsSpan(0, data.Length);
                // Search data for opcode
                while (true)
                {
                    // Get index of first matching opcode
                    var index = slice.IndexOf(opCode.Bytes);
                    // If invalid index, break
                    if (index <= -1 || index > slice.Length)
                    {
                        break;
                    }
                    // Jump to first matching opcode
                    slice = slice.Slice(slice.IndexOf(opCode.Bytes));
                    // If end of data, break
                    if (slice.Length < 4)
                    {
                        break;
                    }
                    // Skip over opcode to data pointer
                    slice = slice.Slice(4);
                    // Get address from pointer
                    var parameterAddress = Convert.ToUInt32(BitConverter.ToString(slice.ToArray(), 0, 4).Replace("-", ""), 16) + 0x24 + opCode.Offset * 8;
                    // If address is out of reach, continue
                    if (parameterAddress > data.Length)
                    {
                        continue;
                    }
                    // Otherwise, get trace ID from address
                    var array = data.AsSpan((int)parameterAddress, 4).ToArray().Reverse();
                    var foundTraceId = BitConverter.ToUInt16(array.ToArray(), 0);
                    // Get offsets
                    var effectPacOffset = (effectPacId - 311) * 10 + 141; // 311 is first custom Effect.pac ID, 141 is first custom trace ID
                    var oldEffectPacOffset = (oldEffectPacId - 311) * 10 + 141; // Multiply by 10 because each trace has 10 entries
                    // Check that trace ID is within range of traces for old Effect.pac
                    if (foundTraceId >= oldEffectPacOffset && foundTraceId < oldEffectPacOffset + 10)
                    {
                        var traceOffset = foundTraceId - oldEffectPacOffset;
                        var newTraceId = effectPacOffset + traceOffset;
                        byte[] newBytes = BitConverter.GetBytes((uint)newTraceId).Reverse().ToArray();
                        newBytes.CopyTo(data, parameterAddress);
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// Update SFX IDs in raw moveset data
        /// </summary>
        /// <param name="data">Raw moveset data</param>
        /// <param name="soundbankId">Soundbank ID to update to</param>
        /// <param name="oldSoundbankId">Soundbank ID to replace</param>
        /// <returns></returns>
        private byte[] UpdateSFXIds(byte[] data, int soundbankId, int oldSoundbankId)
        {
            // Loop through each OpCode
            foreach (var opCode in SfxOpCodes)
            {
                var slice = data.AsSpan(0, data.Length);
                // Search data for opcode
                while (true)
                {
                    // Get index of first matching opcode
                    var index = slice.IndexOf(opCode.Bytes);
                    // If invalid index, break
                    if (index <= -1 || index > slice.Length)
                    {
                        break;
                    }
                    // Jump to first matching opcode
                    slice = slice.Slice(slice.IndexOf(opCode.Bytes));
                    // If end of data, break
                    if (slice.Length < 4)
                    {
                        break;
                    }
                    // Skip over opcode to data pointer
                    slice = slice.Slice(4);
                    // Get address from pointer
                    var parameterAddress = Convert.ToUInt32(BitConverter.ToString(slice.ToArray(), 0, 4).Replace("-", ""), 16) + 0x24 + opCode.Offset * 8;
                    // If address is out of reach, continue
                    if (parameterAddress > data.Length)
                    {
                        continue;
                    }
                    // Otherwise, get SFX ID from address
                    var array = data.AsSpan((int)parameterAddress, 4).ToArray().Reverse();
                    var foundSfxId = BitConverter.ToUInt16(array.ToArray(), 0);
                    // Get offsets
                    var soundbankOffset = (soundbankId - 324) * 165 + 0x4000; // 324 is first custom soundbank ID, 0x4000 is first custom SFX ID
                    var oldSoundbankOffset = (oldSoundbankId - 324) * 165 + 0x4000; // Multiply by 165 because each soundbank has 165 SFX
                    // Check that SFX ID is within range of SFX for old soundbank
                    if (foundSfxId >= oldSoundbankOffset && foundSfxId < oldSoundbankOffset + 165)
                    {
                        var sfxOffset = foundSfxId - oldSoundbankOffset;
                        var newSfxId = soundbankOffset + sfxOffset;
                        byte[] newBytes = BitConverter.GetBytes((uint)newSfxId).Reverse().ToArray();
                        newBytes.CopyTo(data, parameterAddress);
                    }
                }
            }
            return data;
        }
    }
}
