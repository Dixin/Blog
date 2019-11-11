Resize-VHD –Path .\dixinyan-vmxp.vhdx –ToMinimumSize

Convert-VHD -Path .\dixinyan-vmxp.vhdx -DestinationPath .\dixinyan-vmxp.vhd
Convert-VHD -Path .\dixinyan-vmxp.vhd -DestinationPath .\dixinyan-vmxp.min.vhdx