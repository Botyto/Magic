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
		local file_path, root_path

		local subfolder
		if IsKindOf(node, "CodeNode") and IsKindOf(node.parent, "Node") then
			subfolder = node.parent.title .. "/"
			root_path = "../"
		elseif IsKindOf(node, "ClassNode") then
			subfolder = node.title .. "/"
			root_path = "../"
		else
			subfolder = ""
			root_path = ""
		end

		local prefix = node:GetFilePrefix()
		if prefix and prefix ~= "" then
			file_path = string.format("%s%s-%s.md.html", subfolder, node:GetFilePrefix(), node.title)
		else
			file_path = string.format("%s%s.md.html", subfolder, node.title)
		end

		node.file_path = file_path
		node.root_path = root_path
	end
end
