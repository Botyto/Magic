require "Class"
require "CSharpParser"
require "DocGraph"
require "MarkdeepWriter"
BuildClasses()

local folders = { "../../Assets/" }
local output_folder = "../output/"

local parser = CSharpParser:new()
local graph = DocGraph:new()
local writer = MarkdeepWriter:new()

--graph:AddFolders(folders)
graph:AddFile(parser, "../../Assets/Magic/EnergyHolder.cs")
writer:ClearOutput(output_folder)
writer:Output(graph, output_folder)
