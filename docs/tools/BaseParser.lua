require "lib"
require "Class"
require "ClassNode"
require "MethodNode"
require "PropertyNode"
require "ArticleNode"

Class.BaseParser = {
}

function BaseParser:GetFileLines(file)
	local lines = { }
	for line in io.lines(file) do
		table.insert(lines, line)
	end
	return lines
end

function BaseParser:ParseFile(file)
	return { }
end

function BaseParser:SetupNode(node)
	if IsKindOf(node, "PageNode") then
		local prefix = node:GetFilePrefix()
		if prefix and prefix ~= "" then
			node.file_path = string.format("%s-%s.md.html", node:GetFilePrefix(), node.title)
		else
			node.file_path = string.format("%s.md.html", node.title)
		end
	end
end
